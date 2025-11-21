using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cards.Card;
using Core.Cards.Card.Data;
using Core.Cards.Card.Effects;
using Core.Cards.Hand;
using Cysharp.Threading.Tasks;
using Enemy;
using Other;
using Player;
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
        [SerializeField] private PlayerHand _playerHand;
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
                _playerHand.PlayerDefeated -= GameManager.Instance.GameLoose;
                _otherHand.PlayerDefeated -= GameManager.Instance.WinGame;    
            }
        }

        public void StartGame(int[] playerCardIds, int[] otherCardsIds)
        {
            _playerHand.Initialize(playerCardIds);
            _otherHand.Initialize(otherCardsIds);
            
            _playerHand.PlayerDefeated += GameManager.Instance.GameLoose;
            _otherHand.PlayerDefeated += GameManager.Instance.WinGame;
            
            _playerHand.RefillDeck();
            var data = _playerHand.DrawCardsFromDeck(_playerHand.StartingHandSize);
            foreach (var cardData in data) CrateNewCardModel(cardData);
            
            _otherHand.RefillDeck();
            _otherHand.DrawCardsFromDeck(_otherHand.StartingHandSize);

            _enemyBehaviour = new EnemyBehaviour(this, _otherHand, GameManager.Instance.DifficultySettings);
            _enemyBehaviour.PlayTurn();
        }

        public void NextTurn()
        {
            if (GameManager.Instance.GameIsFinished) return;
            
            if (!HasAnyCards(_otherHand, _otherCardSlots))
            {
                GameManager.Instance.WinGame();
                return;
            }
            _otherHand.DrawCardsFromDeck();
            _otherHand.RegenerateHope();
            
            if (!HasAnyCards(_playerHand, _playerCardSlots))
            {
                GameManager.Instance.GameLoose();
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
            GlobalInputBlocker.Instance.DisableInput();
            UIManager.Instance.GetHUDCanvas<GameHUDCanvas>().EnableButton(false);
            await DisplayFinalAttacksAsync();
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                if (!_playerCardSlots[i].IsEmpty)   // Player has card in slot
                {
                    var playerCard = _playerCardSlots[i].Card;
                    var playerEffects = playerCard.CardData.Effects;
                    PlayEffects(playerEffects, TriggerType.CombatStart, GetPlayerContext);
                    
                    if (!_otherCardSlots[i].IsEmpty) // Both slots have cards
                    {
                        var otherCard = _otherCardSlots[i].Card;
                        var otherEffects =  otherCard.CardData.Effects;
                        PlayEffects(otherEffects, TriggerType.CombatStart, GetOtherContext);
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
                                
                                PlayEffects(playerEffects, TriggerType.OnHit, GetPlayerContext); 
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
                                
                                PlayEffects(otherEffects, TriggerType.OnHit, GetOtherContext);
                                otherCard.PlayRandomAnimationReverse();
                                await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                                    cancellationToken:this.destroyCancellationToken);
                                break;
                        }
                        
                        PlayEffects(otherEffects, TriggerType.TurnEnd, GetOtherContext);
                    }
                    else // Player unopposed
                    {
                        _otherHand.TakeDamage(playerCard.FinalAttack);
                        PlayEffects(playerEffects, TriggerType.OnHit, GetPlayerContext);
                        playerCard.PlayRandomAnimation();
                        await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                            cancellationToken:this.destroyCancellationToken);
                    }
                    
                    PlayEffects(playerEffects, TriggerType.TurnEnd, GetPlayerContext);
                }
                else if (!_otherCardSlots[i].IsEmpty) // Enemy unopposed
                {
                    var otherCard = _otherCardSlots[i].Card;
                    var otherEffects = otherCard.CardData.Effects;
                    _playerHand.TakeDamage(otherCard.FinalAttack);
                    PlayEffects(otherEffects, TriggerType.OnHit, GetOtherContext);
                    otherCard.PlayRandomAnimationReverse();
                    
                    await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, 
                        cancellationToken:this.destroyCancellationToken);
                    PlayEffects(otherEffects, TriggerType.TurnEnd, GetOtherContext);
                }

                if (_playerHand.IsDefeated || _otherHand.IsDefeated) break;
            }
            
            ClearFinalAttacks();
            GlobalInputBlocker.Instance.EnableInput();
            UIManager.Instance.GetHUDCanvas<GameHUDCanvas>().EnableButton(true);
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

        private void PlayEffects(Dictionary<TriggerType, CardEffect[]> cardData, TriggerType trigger, Func<BoardContext> contextProvider)
        {
            if (!cardData.TryGetValue(trigger, out var effects)) return;
            foreach (var effect in effects) effect.Execute(contextProvider());
        }

        private BoardContext GetPlayerContext()
        {
            return new BoardContext(_playerCardSlots, _playerHand.CurrentHealth, _playerHand.CurrentHope,
                                    _otherCardSlots, _otherHand.CurrentHealth, _otherHand.CurrentHope);
        }
        
        private BoardContext GetOtherContext()
        {
            return new BoardContext(_otherCardSlots, _otherHand.CurrentHealth, _otherHand.CurrentHope,
                                    _playerCardSlots, _playerHand.CurrentHealth, _playerHand.CurrentHope);
        }

        private void OnDestroy()
        {
            _enemyBehaviour.StateMachine.StopMachine();
        }
    }

    public readonly struct BoardContext
    {
        public readonly CardSlot [] Player;
        public readonly int PlayerHealth;
        public readonly int PlayerHope;
        
        public readonly CardSlot[] Other;
        public readonly int OtherHealth;
        public readonly int OtherHope;

        public BoardContext(CardSlot[] player, int playerHealth, int playerHope, 
                            CardSlot[] other, int otherHealth, int otherHope)
        {
            OtherHope = otherHope;
            Other = other;
            OtherHealth = otherHealth;
            Player = player;
            PlayerHealth = playerHealth;
            PlayerHope = playerHope;
        }
    }
}