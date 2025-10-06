using System.Collections.Generic;
using System.Threading;
using Core.Cards.Card.Data;
using Core.Cards.Card.Effects;
using Core.Cards.Hand;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private Button _playButton;   
        [SerializeField] private Transform _playerCards;
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

        public async UniTask PlayTurnAsync(CardDataBank db, CancellationToken ct = default)
        {
            InputBlocker.Instance.DisableInput();
            await DisplayFinalAttacksAsync(db, ct);
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                if (!_playerCardSlots[i].IsEmpty)   // Player has card in slot
                {
                    var playerCard = _playerCardSlots[i].Card;
                    var playerEffects = playerCard.GetCard(db).Effects;
                    PlayPlayerEffects(playerEffects, TriggerType.CombatStart);
                    
                    if (!_otherCardSlots[i].IsEmpty) // Both slots have cards
                    {
                        var otherCard = _otherCardSlots[i].Card;
                        var otherEffects =  otherCard.GetCard(db).Effects;
                        PlayOtherEffects(otherEffects, TriggerType.CombatStart);
                        // Difference in final power
                        var difference = playerCard.FinalAttack - otherCard.FinalAttack;
                        
                        switch (difference)
                        {
                            // player win
                            case > 0:
                                _otherHand.TakeDamage(difference);
                                PlayPlayerEffects(playerEffects, TriggerType.OnHit); 
                                await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY * 1000f, cancellationToken: ct);
                                break;
                            // player loose
                            case < 0:
                                _playerHand.TakeDamage(-difference);
                                PlayOtherEffects(otherEffects, TriggerType.OnHit);
                                await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY * 1000f, cancellationToken: ct);
                                break;
                        }
                        
                        PlayOtherEffects(otherEffects, TriggerType.NextTurn);
                    }
                    else // Player unopposed
                    {
                        _otherHand.TakeDamage(playerCard.FinalAttack);
                        await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY * 1000f, cancellationToken: ct);
                        PlayPlayerEffects(playerEffects, TriggerType.OnHit);
                    }
                    
                    PlayPlayerEffects(playerEffects, TriggerType.NextTurn);
                }
                else if (!_otherCardSlots[i].IsEmpty) // Enemy unopposed
                {
                    var otherCard = _otherCardSlots[i].Card;
                    var otherEffects = otherCard.GetCard(db).Effects;
                    _playerHand.TakeDamage(otherCard.FinalAttack);
                    PlayOtherEffects(otherEffects, TriggerType.OnHit);
                    
                    await UniTask.WaitForSeconds(BETWEEN_ATTACKS_DELAY * 1000f, cancellationToken: ct);
                    PlayOtherEffects(otherEffects, TriggerType.NextTurn);
                }
            }
            
            ClearFinalAttacks();
            InputBlocker.Instance.EnableInput();
        }

        private async UniTask DisplayFinalAttacksAsync(CardDataBank db, CancellationToken ct = default)
        {
            for (var i = 0; i < _playerCardSlots.Length; i++)
            {
                if (!_playerCardSlots[i].IsEmpty)
                {
                    _playerCardSlots[i].Card.SetRandomFinalAttack(db);
                    await UniTask.WaitForSeconds(FINAL_ATTACK_DISPLAY_DELAY * 1000, cancellationToken:ct);
                }
                
                if (!_otherCardSlots[i].IsEmpty)
                {
                    _playerCardSlots[i].Card.SetRandomFinalAttack(db);
                    if (i < _playerCardSlots.Length) await UniTask.WaitForSeconds(FINAL_ATTACK_DISPLAY_DELAY * 1000, cancellationToken:ct);
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

        private void PlayPlayerEffects(Dictionary<TriggerType, CardEffect[]> cardData, TriggerType trigger)
        {
            if (!cardData.TryGetValue(trigger, out var effects)) return;
            foreach (var effect in effects) effect.Execute(GetPlayerContext());
        }
        
        private void PlayOtherEffects(Dictionary<TriggerType, CardEffect[]> cardData, TriggerType trigger)
        {
            if (!cardData.TryGetValue(trigger, out var effects)) return;
            foreach (var effect in effects) effect.Execute(GetOtherContext());
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