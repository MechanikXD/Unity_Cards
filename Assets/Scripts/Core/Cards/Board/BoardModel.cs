using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Cards.Card;
using Core.Cards.Card.Data;
using Core.Cards.Card.Effects;
using Core.Cards.Hand;
using Cysharp.Threading.Tasks;
using Other;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Cards.Board
{
    public class BoardModel : MonoBehaviour
    {
        private const float FINAL_ATTACK_DISPLAY_DELAY = 0.5f;
        private const float BETWEEN_ATTACKS_DELAY = 1f; 
        [SerializeField] private CardModel _cardPrefab;
        [SerializeField] private CardGroupLayout _layout;
        
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
        }

        public void NextTurn()
        {
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
        }

        private static bool HasAnyCards(PlayerHand hand, CardSlot[] cardSlots) => 
            hand.HasAnyCards || cardSlots.Any(slot => !slot.IsEmpty);

        private void CrateNewCardModel(CardData card)
        {
            var model = Instantiate(_cardPrefab);
            model.Set(card, _playerHand);
            AddCardToLayout((RectTransform)model.transform);
        }
        
        public void AddCardToLayout(RectTransform rect) => _layout.AddChild(rect);

        public void RemoveCardFromLayout(int index) => _layout.RemoveChild(index);

        public async UniTask PlayTurnAsync(CancellationToken ct = default)
        {
            InputBlocker.Instance.DisableInput();
            await DisplayFinalAttacksAsync(ct);
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
                                
                                _otherHand.TakeDamage(difference);
                                PlayEffects(playerEffects, TriggerType.OnHit, GetPlayerContext); 
                                playerCard.PlayRandomAnimation();
                                await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, cancellationToken: ct);
                                break;
                            // player loose
                            case < 0:
                                playerCard.SetFinalAttack(0);
                                otherCard.SetFinalAttack(difference);
                                
                                _playerHand.TakeDamage(-difference);
                                PlayEffects(otherEffects, TriggerType.OnHit, GetOtherContext);
                                otherCard.PlayRandomAnimation();
                                await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, cancellationToken: ct);
                                break;
                        }
                        
                        PlayEffects(otherEffects, TriggerType.TurnEnd, GetOtherContext);
                    }
                    else // Player unopposed
                    {
                        _otherHand.TakeDamage(playerCard.FinalAttack);
                        PlayEffects(playerEffects, TriggerType.OnHit, GetPlayerContext);
                        playerCard.PlayRandomAnimation();
                        await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, cancellationToken: ct);
                    }
                    
                    PlayEffects(playerEffects, TriggerType.TurnEnd, GetPlayerContext);
                }
                else if (!_otherCardSlots[i].IsEmpty) // Enemy unopposed
                {
                    var otherCard = _otherCardSlots[i].Card;
                    var otherEffects = otherCard.CardData.Effects;
                    _playerHand.TakeDamage(otherCard.FinalAttack);
                    PlayEffects(otherEffects, TriggerType.OnHit, GetOtherContext);
                    otherCard.PlayRandomAnimation();
                    
                    await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY, cancellationToken: ct);
                    PlayEffects(otherEffects, TriggerType.TurnEnd, GetOtherContext);
                }

                if (_playerHand.IsDefeated || _otherHand.IsDefeated) break;
            }

            
            ClearFinalAttacks();
            InputBlocker.Instance.EnableInput();
            NextTurn();
        }

        private async UniTask DisplayFinalAttacksAsync(CancellationToken ct = default)
        {
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                if (!_playerCardSlots[i].IsEmpty)
                {
                    _playerCardSlots[i].Card.SetRandomFinalAttack();
                    await UniTask.WaitForSeconds(FINAL_ATTACK_DISPLAY_DELAY, cancellationToken:ct);
                }
                
                if (!_otherCardSlots[i].IsEmpty)
                {
                    _otherCardSlots[i].Card.SetRandomFinalAttack();
                    if (i < _otherCardSlots.Length) await UniTask.WaitForSeconds(FINAL_ATTACK_DISPLAY_DELAY, cancellationToken:ct);
                }
            }
        }

        private void ClearFinalAttacks()
        {
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                if (!_playerCardSlots[i].IsEmpty) _playerCardSlots[i].Card.ClearFinalAttack();
                
                if (!_otherCardSlots[i].IsEmpty) _playerCardSlots[i].Card.ClearFinalAttack();
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