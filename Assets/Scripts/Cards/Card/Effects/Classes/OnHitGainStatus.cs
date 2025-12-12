using Cards.Board;
using UnityEngine;

namespace Cards.Card.Effects.Classes
{
    [CreateAssetMenu(fileName = "OnHitGainStatus", menuName = "ScriptableObjects/Card Effect/OnHitGainStatus")]
    public class OnHitGainStatus : CardEffect
    {
        [SerializeField] private string _statusName;
        [SerializeField] private int _initialApplyValue;
        [SerializeField] private int _statusAmount;
        
        public override void Execute(BoardContext context)
        {
            var slot = context.Player[context.Index];
            if (slot.IsEmpty) return;

            if (!slot.Card.LocalStatuses.TryAdd(_statusName, _initialApplyValue)) 
                slot.Card.LocalStatuses[_statusName] += _statusAmount;
        }
    }
}