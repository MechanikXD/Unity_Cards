using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cards.Card;
using Core.Cards.Card.Data;
using Core.Cards.Card.Effects;
using Core.Cards.Hand;
using Core.SessionStorage;
using Cysharp.Threading.Tasks;
using Enemy;
using Other;
using Player;
using Player.Progression.Buffs;
using Player.Progression.SaveStates;
using TMPro;
using UI;
using UI.View.GameView;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Cards.Board
{
    public class BoardModel : MonoBehaviour
    {
        private const int CARD_SORTING_GROUP = 3;
        private const float FINAL_ATTACK_DISPLAY_DELAY = 0.5f;
        private const float BETWEEN_ATTACKS_DELAY = 1f; 
        [SerializeField] private CardModel _cardPrefab;
        [SerializeField] private CardGroupLayout _layout;
        private EnemyBehaviour _enemyBehaviour;
        
        [Header("Player")]
        [SerializeField] private CardSlot[] _playerCardSlots;
        [SerializeField] private PlayerStatView _playerStatView;
        private PlayerHand _playerHand;
        [Space]
        [SerializeField] private Image _playerHpFill;
        [SerializeField] private TMP_Text _playerHpText;
        [Space]
        [SerializeField] private Image _playerHopeFill;
        [SerializeField] private TMP_Text _playerHopeText;
        
        [Header("Other")]
        [SerializeField] private CardSlot[] _otherCardSlots;
        [SerializeField] private PlayerHand _otherHand;
        [Space]
        [SerializeField] private Image _otherHpFill;
        [SerializeField] private TMP_Text _otherHpText;
        [Space]
        [SerializeField] private Image _otherHopeFill;
        [SerializeField] private TMP_Text _otherHopeText;

        public CardModel CardPrefab => _cardPrefab;
        public CardSlot[] PlayerSlots => _playerCardSlots;
        public CardSlot[] EnemySlots => _otherCardSlots;
        public static event Action TurnStarted;
        
        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                _playerHand.PlayerDefeated -= GameManager.Instance.LooseAct;
                _otherHand.PlayerDefeated -= GameManager.Instance.WinAct;    
            }
        }

        public void StartGame(PlayerHand player, EnemyDifficultySettings settings)
        {
            _playerHand = player;
            _playerHand.SetStatView(_playerStatView);
            var otherDeck = settings.GetDeck();
            _otherHand.Initialize(otherDeck);
            
            _playerHand.PlayerDefeated += GameManager.Instance.LooseAct;
            _otherHand.PlayerDefeated += GameManager.Instance.WinAct;
            
            _playerHand.UpdateStatView(true);
            _otherHand.UpdateStatView(true);
            
            StartAct(settings);
        }

        public void LoadFromSerialized(SerializableBoard board, SerializablePlayerHand player)
        {
            var db = CardDataProvider.DataBank;
            foreach (var card in board.PlayerCards)
            {
                if (card == null) continue;
                
                var newModel = Instantiate(_cardPrefab);
                newModel.Set(card.Value.ToCardData(db), null);
                newModel.Animator.enabled = true;
            }
            
            foreach (var card in board.EnemyCards)
            {
                if (card == null) continue;
                
                var newModel = Instantiate(_cardPrefab);
                newModel.Set(card.Value.ToCardData(db), null);
                newModel.Animator.enabled = true;
            }

            foreach (var card in player._hand) CrateNewCardModel(card.ToCardData(db));
        }

        public void FinishAct()
        {
            foreach (var slot in PlayerSlots)
            {
                if (slot.IsEmpty) continue;

                var card = slot.Detach();
                Destroy(card.gameObject);
            }
            _playerHand.ResetAll();
            _layout.RemoveALl();
            
            foreach (var slot in EnemySlots)
            {
                if (slot.IsEmpty) continue;

                var card = slot.Detach();
                Destroy(card.gameObject);
            }
            _otherHand.ResetAll();
        }

        private void StartAct(EnemyDifficultySettings settings)
        {
            var storage = GameStorage.Instance;
            storage.AdvanceAct();
            
            _playerHand.RefillDeck();
            storage.PlayerBuffs.ApplyAll(_playerHand, ActivationType.ActStart);
            
            var data = _playerHand.DrawCardsFromDeck(_playerHand.StartingHandSize);
            foreach (var cardData in data) CrateNewCardModel(cardData);
            
            storage.LoadEnemyBuffs(_otherHand);
            _otherHand.RefillDeck();
            _otherHand.DrawCardsFromDeck(_otherHand.StartingHandSize);
            _enemyBehaviour = new EnemyBehaviour(this, _otherHand, settings);
            _enemyBehaviour.PlayTurn();
        }

        public void NextTurn()
        {
            if (GameManager.Instance.ActIsFinished) return;
            
            if (!HasAnyCards(_otherHand, _otherCardSlots))
            {
                GameManager.Instance.WinAct();
                return;
            }
            _otherHand.DrawCardsFromDeck();
            _otherHand.RegenerateHope();
            
            if (!HasAnyCards(_playerHand, _playerCardSlots))
            {
                GameManager.Instance.LooseAct();
                return;
            }
            var data = _playerHand.DrawCardsFromDeck();
            foreach (var cardData in data) CrateNewCardModel(cardData);
            _playerHand.RegenerateHope();
            TurnStarted?.Invoke();
            _enemyBehaviour.PlayTurn();
        }

        private static bool HasAnyCards(PlayerHand hand, CardSlot[] cardSlots) => 
            hand.HasAnyCards || cardSlots.Any(slot => !slot.IsEmpty);

        private void CrateNewCardModel(CardData card)
        {
            var model = Instantiate(_cardPrefab);
            model.Set(card, _playerHand);
            AddCardToLayout(model);
        }
        
        public void AddCardToLayout(CardModel rect) => _layout.AddChild(rect);

        public void RemoveCardFromLayout(int index) => _layout.RemoveChild(index);

        public async UniTask PlayTurnAsync()
        {
            HideInfoOnClick.HideAll();
            GlobalInputBlocker.Instance.DisableInput();
            UIManager.Instance.GetHUDCanvas<GameHUDView>().EnableButton(false);
            
            GameStorage.Instance.PlayerBuffs.ApplyAll(_playerHand, ActivationType.CombatStart);
            GameStorage.Instance.EnemyBuffs.ApplyAll(_otherHand, ActivationType.CombatStart);
            await DisplayFinalAttacksAsync();
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                var currentIndex = i;   // To capture value
                if (!_playerCardSlots[i].IsEmpty)   // Player has card in slot
                {
                    var playerCard = _playerCardSlots[i].Card;
                    var playerEffects = playerCard.Data.Effects;
                    
                    PlayEffects(playerEffects, TriggerType.CombatStart, () => GetPlayerContext(currentIndex));
                    
                    if (!_otherCardSlots[i].IsEmpty) // Both slots have cards
                    {
                        var otherCard = _otherCardSlots[i].Card;
                        var otherEffects =  otherCard.Data.Effects;
                        PlayEffects(otherEffects, TriggerType.CombatStart, () => GetOtherContext(currentIndex));
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
                                
                                otherCard.TakeDamage(difference);
                                if (otherCard.IsDefeated)
                                {
                                    var model = _otherCardSlots[i].Detach();
                                    Destroy(model.gameObject);
                                }
                                
                                PlayEffects(playerEffects, TriggerType.OnHit, () => GetPlayerContext(currentIndex)); 
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
                                
                                playerCard.TakeDamage(-difference);
                                if (playerCard.IsDefeated)
                                {
                                    var model = _playerCardSlots[i].Detach();
                                    Destroy(model.gameObject);
                                }
                                
                                PlayEffects(otherEffects, TriggerType.OnHit, () => GetOtherContext(currentIndex));
                                otherCard.PlayRandomAnimationReverse();
                                await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                                    cancellationToken:this.destroyCancellationToken);
                                break;
                        }
                        
                        PlayEffects(otherEffects, TriggerType.TurnEnd, () => GetOtherContext(currentIndex));
                    }
                    else // Player unopposed
                    {
                        _otherHand.TakeDamage(playerCard.FinalAttack);
                        PlayEffects(playerEffects, TriggerType.OnHit, () => GetPlayerContext(currentIndex));
                        playerCard.PlayRandomAnimation();
                        await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                            cancellationToken:this.destroyCancellationToken);
                    }
                    
                    PlayEffects(playerEffects, TriggerType.TurnEnd, () => GetPlayerContext(currentIndex));
                }
                else if (!_otherCardSlots[i].IsEmpty) // Enemy unopposed
                {
                    var otherCard = _otherCardSlots[i].Card;
                    var otherEffects = otherCard.Data.Effects;
                    _playerHand.TakeDamage(otherCard.FinalAttack);
                    PlayEffects(otherEffects, TriggerType.OnHit, () => GetOtherContext(currentIndex));
                    otherCard.PlayRandomAnimationReverse();
                    
                    await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                        cancellationToken:this.destroyCancellationToken);
                    PlayEffects(otherEffects, TriggerType.TurnEnd, () => GetOtherContext(currentIndex));
                }

                if (_playerHand.IsDefeated || _otherHand.IsDefeated) break;
            }
            
            ClearFinalAttacks();
            GlobalInputBlocker.Instance.EnableInput();
            UIManager.Instance.GetHUDCanvas<GameHUDView>().EnableButton(true);
            NextTurn();
        }

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
                
                if (!_otherCardSlots[i].IsEmpty)
                {
                    _otherCardSlots[i].Card.SetRandomFinalAttack();
                    if (i < _otherCardSlots.Length) await UniTask.WaitForSeconds(FINAL_ATTACK_DISPLAY_DELAY, 
                        cancellationToken:this.destroyCancellationToken);
                }
            }
        }

        private void ClearFinalAttacks()
        {
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                if (!_playerCardSlots[i].IsEmpty) _playerCardSlots[i].Card.ClearFinalAttack();
                
                if (!_otherCardSlots[i].IsEmpty) _otherCardSlots[i].Card.ClearFinalAttack();
            }
        }

        private void PlayEffects(Dictionary<TriggerType, List<CardEffect>> cardData, TriggerType trigger, Func<BoardContext> contextProvider)
        {
            if (!cardData.TryGetValue(trigger, out var effects)) return;
            foreach (var effect in effects) effect.Execute(contextProvider());
        }

        private BoardContext GetPlayerContext(int index)
        {
            return new BoardContext(_playerCardSlots, index, _playerHand, _otherCardSlots, _otherHand);
        }
        
        private BoardContext GetOtherContext(int index)
        {
            return new BoardContext(_otherCardSlots, index, _otherHand, _playerCardSlots, _playerHand);
        }

        private void OnDestroy()
        {
            _enemyBehaviour.StateMachine.StopMachine();
        }
    }

    public readonly struct BoardContext
    {
        public readonly CardSlot[] Player;
        public readonly PlayerHand PlayerHand;

        public readonly CardSlot[] Other;
        public readonly PlayerHand OtherHand;
        public readonly int Index;

        public BoardContext(CardSlot[] player, int index, PlayerHand playerHand, 
                            CardSlot[] other, PlayerHand otherHand)
        {
            Index = index;
            Other = other;
            Player = player;
            PlayerHand = playerHand;
            OtherHand = otherHand;
        }
    }
}