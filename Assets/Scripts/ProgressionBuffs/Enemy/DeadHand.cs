using Cards.Card.Data;
using Cards.Hand;
using ProgressionBuffs.Scriptables;
using Structure.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "DeadHand", menuName = "ScriptableObjects/Buff/Enemy/DeadHand")]
    public class DeadHand : EnemyBuff
    {
        [SerializeField] private CardData _cardData;
        [SerializeField] private int _cardCount;
        
        public override void Apply(PlayerData data)
        {
            var newCards = new CardData[_cardCount];
            for (var i = 0; i < _cardCount; i++) newCards[i] = _cardData;
            
            SessionManager.Instance.PlayerData.Deck.AddRange(newCards);
        }
    }
}