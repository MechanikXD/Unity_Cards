#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Card;
using Cards.Card.Data;
using Cards.Card.Effects;
using Cards.Hand;
using Cysharp.Threading.Tasks;
using Enemy;
using Other.Interactions;
using ProgressionBuffs;
using SaveLoad;
using SaveLoad.Serializeables;
using Structure;
using Structure.Managers;
using TMPro;
using UI;
using UI.View.GameView;
using UnityEngine;

namespace Cards.Board
{
    /// <summary>
    /// Model representing a board on game scene; also used as scene controller 
    /// </summary>
    public class BoardModel : MonoBehaviour, IGameSerializable<BoardState>
    {
        private const int CARD_SORTING_GROUP = 3;
        private const float FINAL_ATTACK_DISPLAY_DELAY = 0.5f;
        private const float BETWEEN_ATTACKS_DELAY = 1f;
        private const float NUMBER_DECREASE_SPEED = 0.2f;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [SerializeField] private CardModel _cardPrefab;
        [SerializeField] private CardGroupLayout _layout;
        private EnemyBehaviour _enemyBehaviour;
        private ObjectPool<CardModel> _modelPool;
        [SerializeField] private CanvasGroup _turnDisplay;
        [SerializeField] private TMP_Text _turnCounter;
        [SerializeField] private float _turnShowSpeed = 5f;
        [SerializeField] private float _turnStayTime = 0.5f;
        [SerializeField] private float _turnHideSpeed = 2f;
        private int _currentTurn;

        [Header("Player")]
        [SerializeField] private Vector3 _cardSpawn;
        [SerializeField] private CardSlot[] _playerCardSlots;
        [SerializeField] private PlayerStatView _playerStatView;
        // Workaround to ease access to player
        private static PlayerData PlayerData => SessionManager.Instance.PlayerData;

        [Header("Enemy")]
        [SerializeField] private Vector3 _slotRelativeCardSpawn;
        [SerializeField] private CardSlot[] _enemyCardSlots;
        [SerializeField] private PlayerData _enemyData;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public CardSlot[] PlayerSlots => _playerCardSlots;
        public CardSlot[] EnemySlots => _enemyCardSlots;
        
        private void OnDisable() => UnsubscribeFromEvents();
        
        #region Game Flow Controll
        
        public void StartGame(EnemyDifficultySettings settings, bool startAct=true)
        {
            PlayerData.SetStatView(_playerStatView);
            InitializeEnemy(settings);
            
            PlayerData.PlayerDefeated += GameManager.Instance.LooseAct;
            _enemyData.PlayerDefeated += GameManager.Instance.WinAct;
            
            PlayerData.UpdateStatView(true);
            _enemyData.UpdateStatView(true);

            _modelPool = new ObjectPool<CardModel>(_cardPrefab, 15, null, null);

            _currentTurn = 0;
            _turnCounter.SetText(_currentTurn.ToString());
            DisplayTurn().Forget();
            
            if (startAct) StartAct();
        }

        private void StartAct()
        {
            var storage = SessionManager.Instance;
            storage.AdvanceAct();
            
            PlayerData.RefillDeck();
            storage.PlayerBuffs.ApplyAll(PlayerData, ActivationType.ActStart);
            
            var data = PlayerData.DrawCardsFromDeck(PlayerData.StartingHandSize);
            foreach (var cardData in data) CrateNewCardModel(cardData);
            
            _enemyData.RefillDeck();
            _enemyData.DrawCardsFromDeck(_enemyData.StartingHandSize);
            
            _enemyBehaviour.PlayTurn();
        }

        // Player at each turn start
        private void NextTurn()
        {
            if (GameManager.Instance.ActIsFinished) return;
            
            if (!HasAnyCards(_enemyData, _enemyCardSlots))
            {
                GameManager.Instance.WinAct();
                return;
            }
            _enemyData.DrawCardsFromDeck();
            _enemyData.RegenerateLight();
            
            if (!HasAnyCards(PlayerData, _playerCardSlots))
            {
                GameManager.Instance.LooseAct();
                return;
            }
            
            _currentTurn++;
            _turnCounter.SetText(_currentTurn.ToString());
            SessionManager.Instance.IncrementStatistics("Total Turns");
            DisplayTurn().Forget();
            
            var data = PlayerData.DrawCardsFromDeck();
            foreach (var cardData in data) CrateNewCardModel(cardData);
            PlayerData.RegenerateLight();
            _enemyBehaviour.PlayTurn();
        }
        
        // Scary method to play turn with all effects/animation/etc.
        public async UniTask PlayTurnAsync()
        {
            HideInfoOnClick.HideAll();
            GlobalInputBlocker.Instance.DisableInput();
            UIManager.Instance.GetHUDCanvas<GameHUDView>().EnablePlayButton(false);
            
            PlayerData.DoCombatStartEvents();
            _enemyData.DoCombatStartEvents();
            SessionManager.Instance.PlayerBuffs.ApplyAll(PlayerData, ActivationType.CombatStart);
            SessionManager.Instance.EnemyBuffs.ApplyAll(_enemyData, ActivationType.CombatStart);
            await DisplayFinalAttacksAsync();
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                var currentIndex = i;   // To capture value
                if (!_playerCardSlots[i].IsEmpty)   // Player has card in slot
                {
                    var playerCard = _playerCardSlots[i].Card;
                    var playerEffects = playerCard.Data.Effects;
                    
                    PlayEffects(playerEffects, TriggerType.CombatStart, () => GetPlayerContext(currentIndex));
                    
                    if (!_enemyCardSlots[i].IsEmpty) // Both slots have cards
                    {
                        var otherCard = _enemyCardSlots[i].Card;
                        var otherEffects =  otherCard.Data.Effects;
                        PlayEffects(otherEffects, TriggerType.CombatStart, () => GetEnemyContext(currentIndex));
                        // Difference in final power
                        var difference = playerCard.FinalAttack - otherCard.FinalAttack;
                        
                        switch (difference)
                        {
                            // player win
                            case > 0:
                                playerCard.SetFinalAttack(difference);
                                otherCard.SetFinalAttack(0);

                                playerCard.SortingGroup.sortingOrder = CARD_SORTING_GROUP + 1;
                                otherCard.SortingGroup.sortingOrder = CARD_SORTING_GROUP;

                                var i1 = i;
                                playerCard.AddActionOnHit(() =>
                                {
                                    otherCard.TakeDamage(difference);
                                    if (otherCard.IsDefeated)
                                    {
                                        var model = _enemyCardSlots[i1].Detach();
                                        _modelPool.Return(model);
                                        SessionManager.Instance.IncrementStatistics("Cards Defeated");
                                    }
                                    PlayEffects(playerEffects, TriggerType.OnHit, () => GetPlayerContext(currentIndex));
                                });
                                
                                playerCard.PlayRandomAnimation();
                                await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                                    cancellationToken:this.destroyCancellationToken);
                                break;
                            // player loose
                            case < 0:
                                playerCard.SetFinalAttack(0);
                                otherCard.SetFinalAttack(-difference);
                                
                                playerCard.SortingGroup.sortingOrder = CARD_SORTING_GROUP;
                                otherCard.SortingGroup.sortingOrder = CARD_SORTING_GROUP + 1;

                                var i2 = i;
                                otherCard.AddActionOnHit(() =>
                                {
                                    playerCard.TakeDamage(-difference);
                                    if (playerCard.IsDefeated)
                                    {
                                        var model = _playerCardSlots[i2].Detach();
                                        _modelPool.Return(model);
                                    }
                                
                                    PlayEffects(otherEffects, TriggerType.OnHit, () => GetEnemyContext(currentIndex));
                                });
                                
                                otherCard.PlayRandomAnimationReverse();
                                await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                                    cancellationToken:this.destroyCancellationToken);
                                break;
                        }
                        
                        PlayEffects(otherEffects, TriggerType.TurnEnd, () => GetEnemyContext(currentIndex));
                    }
                    else // Player unopposed
                    {
                        playerCard.AddActionOnHit(() =>
                        {
                            _enemyData.TakeDamage(playerCard.FinalAttack);
                            PlayEffects(playerEffects, TriggerType.OnHit, () => GetPlayerContext(currentIndex));
                        });
                        
                        playerCard.PlayRandomAnimation();
                        await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                            cancellationToken:this.destroyCancellationToken);
                    }
                    
                    PlayEffects(playerEffects, TriggerType.TurnEnd, () => GetPlayerContext(currentIndex));
                }
                else if (!_enemyCardSlots[i].IsEmpty) // Enemy unopposed
                {
                    var otherCard = _enemyCardSlots[i].Card;
                    var otherEffects = otherCard.Data.Effects;
                    
                    otherCard.AddActionOnHit(() =>
                    {
                        PlayerData.TakeDamage(otherCard.FinalAttack);
                        PlayEffects(otherEffects, TriggerType.OnHit, () => GetEnemyContext(currentIndex));
                    });
                    
                    otherCard.PlayRandomAnimationReverse();
                    await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                        cancellationToken:this.destroyCancellationToken);
                    PlayEffects(otherEffects, TriggerType.TurnEnd, () => GetEnemyContext(currentIndex));
                }

                if (PlayerData.IsDefeated || _enemyData.IsDefeated) break;
            }
            
            ClearFinalAttacks();
            GlobalInputBlocker.Instance.EnableInput();
            UIManager.Instance.GetHUDCanvas<GameHUDView>().EnablePlayButton(true);
            NextTurn();
        }
        
        #endregion

        #region Animtion-related

        private async UniTask DisplayFinalAttacksAsync()
        {
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                if (!_playerCardSlots[i].IsEmpty)
                {
                    _playerCardSlots[i].Card.SetRandomFinalAttack();
                    await UniTask.WaitForSeconds(FINAL_ATTACK_DISPLAY_DELAY, 
                        cancellationToken:this.destroyCancellationToken);
                }
                
                if (!_enemyCardSlots[i].IsEmpty)
                {
                    _enemyCardSlots[i].Card.SetRandomFinalAttack();
                    if (i < _enemyCardSlots.Length) await UniTask.WaitForSeconds(FINAL_ATTACK_DISPLAY_DELAY, 
                        cancellationToken:this.destroyCancellationToken);
                }
            }
        }

        private async UniTask LowerFinalAttackValueAsync(CardModel model, int toNumber)
        {
            if (model.FinalAttack <= toNumber) return;

            while (model.FinalAttack > toNumber)
            {
                model.SetFinalAttack(model.FinalAttack - 1);
                await UniTask.WaitForSeconds(NUMBER_DECREASE_SPEED);
            }
        }

        private void ClearFinalAttacks()
        {
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                if (!_playerCardSlots[i].IsEmpty) _playerCardSlots[i].Card.ClearFinalAttack();
                
                if (!_enemyCardSlots[i].IsEmpty) _enemyCardSlots[i].Card.ClearFinalAttack();
            }
        }

        private async UniTask DisplayTurn(float valueSnap = 0.05f)
        {
            while (Mathf.Abs(1f - _turnDisplay.alpha) > valueSnap)
            {
                _turnDisplay.alpha = Mathf.Lerp(_turnDisplay.alpha, 1f, _turnShowSpeed * Time.deltaTime);
                await UniTask.NextFrame(_turnDisplay.GetCancellationTokenOnDestroy());
            }

            _turnDisplay.alpha = 1f;
            await UniTask.WaitForSeconds(_turnStayTime, 
                cancellationToken:_turnDisplay.GetCancellationTokenOnDestroy());
            while (_turnDisplay.alpha > valueSnap)
            {
                _turnDisplay.alpha = Mathf.Lerp(_turnDisplay.alpha, 0f, _turnHideSpeed * Time.deltaTime);
                await UniTask.NextFrame(_turnDisplay.GetCancellationTokenOnDestroy());
            }

            _turnDisplay.alpha = 0f;
        } 

        #endregion

        #region Player Data and Layout

        private static bool HasAnyCards(PlayerData data, CardSlot[] cardSlots) => 
            data.HasAnyCards || cardSlots.Any(slot => !slot.IsEmpty);
        
        // Player only method to create and add new card to layout
        private void CrateNewCardModel(CardData card)
        {
            var model = _modelPool.Pull();
            model.Set(card, PlayerData);
            model.transform.position = _cardSpawn;
            model.Animator.enabled = false;
            model.Controller.Interactable = true;
            AddCardToLayout(model);
        }
        public void AddCardToLayout(CardModel rect) => _layout.AddChild(rect);
        public void RemoveCardFromLayout(int index) => _layout.RemoveChild(index);

        #endregion

        // Check for other models containing 'RequestMove' check
        public bool AnyRequireMove()
        {
            foreach (var slot in PlayerSlots)
            {
                if (!slot.IsEmpty && slot.Card.RequestMove)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void PlaceEnemyCard(CardData data, int index)
        {
            var thisSlot = EnemySlots[index];
            if (!thisSlot.IsEmpty) return;

            var newCard = _modelPool.Pull();
            newCard.transform.position = thisSlot.transform.position + _slotRelativeCardSpawn;
            newCard.Animator.enabled = true;
            newCard.Controller.Interactable = false;
            newCard.Set(data, null);
            
            thisSlot.Attach(newCard);
        }
        
        // To play effect of certain activation type...
        private static void PlayEffects(Dictionary<TriggerType, List<CardEffect>> cardData, TriggerType trigger, Func<BoardContext> contextProvider)
        {
            if (!cardData.TryGetValue(trigger, out var effects)) return;
            foreach (var effect in effects) effect.Execute(contextProvider());
        }
        
        public BoardState SerializeSelf()
        {
            var playerCards = new SerializableCardData?[_playerCardSlots.Length];
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                playerCards[i] = _playerCardSlots[i].IsEmpty
                    ? null
                    : _playerCardSlots[i].Card.Data.SerializeSelf();
            }
            
            var enemyCards = new SerializableCardData?[_enemyCardSlots.Length];
            for (var i = 0; i < _enemyCardSlots.Length; i++)
            {
                enemyCards[i] = _enemyCardSlots[i].IsEmpty
                    ? null
                    : _enemyCardSlots[i].Card.Data.SerializeSelf();
            }

            var board = new SerializableBoard(playerCards, enemyCards, _enemyBehaviour.Settings.DifficultyName);

            return new BoardState(board, _enemyData.SerializeSelf());
        }

        public void Deserialize(BoardState self)
        {
            _modelPool = new ObjectPool<CardModel>(_cardPrefab, 15, null, null);
            for (var i = 0; i < self.Board._playerCards.Length; i++)
            {
                var card = self.Board._playerCards[i];
                if (card == null) continue;
                
                var newModel = _modelPool.Pull();
                newModel.Set(card.ToCardData(), null);
                newModel.transform.position = _cardSpawn;
                newModel.Animator.enabled = true;
                _playerCardSlots[i].Attach(newModel, true);
            }
            
            for (var i = 0; i < self.Board._enemyCards.Length; i++)
            {
                var card = self.Board._enemyCards[i];
                if (card == null) continue;
                
                var newModel = _modelPool.Pull();
                newModel.Set(card.ToCardData(), null);
                newModel.transform.position =
                    _enemyCardSlots[i].transform.position + _slotRelativeCardSpawn;
                newModel.Animator.enabled = true;
                _enemyCardSlots[i].Attach(newModel, true);
            }
            
            SessionManager.Instance.SetSettings(self.Board._enemyDifficultyName);
            _enemyData.Deserialize(self.Enemy);

            foreach (var card in PlayerData.CardsInHand) CrateNewCardModel(card);
        }
        
        private void InitializeEnemy(EnemyDifficultySettings settings)
        {
            var otherDeck = settings.GetDeck();
            _enemyData.Initialize(otherDeck);
            SessionManager.Instance.LoadEnemyBuffs(_enemyData);
            _enemyBehaviour = new EnemyBehaviour(this, _enemyData, settings);
        }
        
        // Board context from player card 
        private BoardContext GetPlayerContext(int index) => 
            new BoardContext(_playerCardSlots, index, PlayerData, _enemyCardSlots, _enemyData);

        // Board context from enemy card
        private BoardContext GetEnemyContext(int index) => 
            new BoardContext(_enemyCardSlots, index, _enemyData, _playerCardSlots, PlayerData);
        
        private void UnsubscribeFromEvents()
        {
            if (GameManager.Instance != null)
            {
                if (SessionManager.Instance !=null) PlayerData.PlayerDefeated -= GameManager.Instance.LooseAct;
                _enemyData.PlayerDefeated -= GameManager.Instance.WinAct;    
            }
        }
    }

    /// <summary>
    /// Context distributed to card effects, Card slots and player data are classes thus references
    /// </summary>
    public readonly struct BoardContext
    {
        public readonly CardSlot[] Player;
        public readonly PlayerData PlayerData;

        public readonly CardSlot[] Other;
        public readonly PlayerData OtherData;
        public readonly int Index;

        public BoardContext(CardSlot[] player, int index, PlayerData playerData, 
                            CardSlot[] other, PlayerData otherData)
        {
            Index = index;
            Other = other;
            Player = player;
            PlayerData = playerData;
            OtherData = otherData;
        }
    }
}