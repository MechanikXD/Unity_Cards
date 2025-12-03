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
    public class BoardModel : MonoBehaviour, IGameSerializable<BoardState>
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
        private static PlayerHand PlayerHand => GameStorage.Instance.PlayerHand;
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
        
        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                PlayerHand.PlayerDefeated -= GameManager.Instance.LooseAct;
                _otherHand.PlayerDefeated -= GameManager.Instance.WinAct;    
            }
        }

        public void StartGame(EnemyDifficultySettings settings, bool startAct=true)
        {
            PlayerHand.SetStatView(_playerStatView);
            
            PlayerHand.PlayerDefeated += GameManager.Instance.LooseAct;
            _otherHand.PlayerDefeated += GameManager.Instance.WinAct;
            
            PlayerHand.UpdateStatView(true);
            _otherHand.UpdateStatView(true);

            if (startAct)
            {
                var otherDeck = settings.GetDeck();
                _otherHand.Initialize(otherDeck);
                StartAct(settings);
            }
        }

        public void FinishAct()
        {
            foreach (var slot in PlayerSlots)
            {
                if (slot.IsEmpty) continue;

                var card = slot.Detach();
                Destroy(card.gameObject);
            }
            PlayerHand.ResetAll();
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
            
            PlayerHand.RefillDeck();
            storage.PlayerBuffs.ApplyAll(PlayerHand, ActivationType.ActStart);
            
            var data = PlayerHand.DrawCardsFromDeck(PlayerHand.StartingHandSize);
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
            
            if (!HasAnyCards(PlayerHand, _playerCardSlots))
            {
                GameManager.Instance.LooseAct();
                return;
            }
            var data = PlayerHand.DrawCardsFromDeck();
            foreach (var cardData in data) CrateNewCardModel(cardData);
            PlayerHand.RegenerateHope();
            _enemyBehaviour.PlayTurn();
        }

        private static bool HasAnyCards(PlayerHand hand, CardSlot[] cardSlots) => 
            hand.HasAnyCards || cardSlots.Any(slot => !slot.IsEmpty);

        private void CrateNewCardModel(CardData card)
        {
            var model = Instantiate(_cardPrefab);
            model.Set(card, PlayerHand);
            AddCardToLayout(model);
        }
        
        public void AddCardToLayout(CardModel rect) => _layout.AddChild(rect);

        public void RemoveCardFromLayout(int index) => _layout.RemoveChild(index);

        public async UniTask PlayTurnAsync()
        {
            HideInfoOnClick.HideAll();
            GlobalInputBlocker.Instance.DisableInput();
            UIManager.Instance.GetHUDCanvas<GameHUDView>().EnableButton(false);
            
            GameStorage.Instance.PlayerBuffs.ApplyAll(PlayerHand, ActivationType.CombatStart);
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
                    PlayerHand.TakeDamage(otherCard.FinalAttack);
                    PlayEffects(otherEffects, TriggerType.OnHit, () => GetOtherContext(currentIndex));
                    otherCard.PlayRandomAnimationReverse();
                    
                    await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                        cancellationToken:this.destroyCancellationToken);
                    PlayEffects(otherEffects, TriggerType.TurnEnd, () => GetOtherContext(currentIndex));
                }

                if (PlayerHand.IsDefeated || _otherHand.IsDefeated) break;
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
        
        public BoardState SerializeSelf()
        {
            var playerCards = new SerializableCardData?[_playerCardSlots.Length];
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                playerCards[i] = _playerCardSlots[i].IsEmpty
                    ? null
                    : _playerCardSlots[i].Card.Data.SerializeSelf();
            }
            
            var enemyCards = new SerializableCardData?[_otherCardSlots.Length];
            for (var i = 0; i < _otherCardSlots.Length; i++)
            {
                enemyCards[i] = _otherCardSlots[i].IsEmpty
                    ? null
                    : _otherCardSlots[i].Card.Data.SerializeSelf();
            }

            var board = new SerializableBoard(playerCards, enemyCards);

            return new BoardState(board, _otherHand.SerializeSelf());
        }

        public void Deserialize(BoardState self)
        {
            // TODO: Have to attach to slots
            foreach (var card in self.Board._playerCards)
            {
                if (card == null) continue;
                
                var newModel = Instantiate(_cardPrefab);
                newModel.Set(card.ToCardData(), null);
                newModel.Animator.enabled = true;
            }
            
            foreach (var card in self.Board._enemyCards)
            {
                if (card == null) continue;
                
                var newModel = Instantiate(_cardPrefab);
                newModel.Set(card.ToCardData(), null);
                newModel.Animator.enabled = true;
            }
            
            _otherHand.Deserialize(self.Enemy);

            foreach (var card in PlayerHand.CardsInHand) CrateNewCardModel(card);
        }
        
        private BoardContext GetPlayerContext(int index)
        {
            return new BoardContext(_playerCardSlots, index, PlayerHand, _otherCardSlots, _otherHand);
        }
        
        private BoardContext GetOtherContext(int index)
        {
            return new BoardContext(_otherCardSlots, index, _otherHand, _playerCardSlots, PlayerHand);
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