using Cards.Board;
using UnityEngine;

namespace Cards.Card.Effects.Classes
{
    [CreateAssetMenu(fileName = "BonusDamagePerStatus", menuName = "ScriptableObjects/Card Effect/BonusDamagePerStatus")]
    public class BonusDamagePerStatus : CardEffect
    {
        [SerializeField] private string _statusName;
        [SerializeField] private int _bonusDamage;
        [SerializeField] private float _multiply = 1f;
        
        public override void Execute(BoardContext context)
        {
            var oppositeSlot = context.Other[context.Index];
            if (oppositeSlot.IsEmpty) return;

            if (oppositeSlot.Card.LocalStatuses.TryGetValue(_statusName, out var statusCount))
                oppositeSlot.Card.TakeDamage((int)(statusCount * _multiply));
        }
    }
}