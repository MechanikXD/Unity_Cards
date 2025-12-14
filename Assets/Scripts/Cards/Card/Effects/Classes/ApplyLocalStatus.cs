using Cards.Board;
using UnityEngine;

namespace Cards.Card.Effects.Classes
{
    [CreateAssetMenu(fileName = "ApplyLocalStatus", menuName = "ScriptableObjects/Card Effect/ApplyLocalStatus")]
    public class ApplyLocalStatus : CardEffect
    {
        [SerializeField] private string _statusName;
        [SerializeField] private int _initialApplyValue;
        [SerializeField] private int _statusAmount;
        
        public override void Execute(BoardContext context)
        {
            var slot = context.Other[context.Index];
            if (slot.IsEmpty) return;

            if (!slot.Card.LocalStatuses.TryAdd(_statusName, _initialApplyValue)) 
                slot.Card.LocalStatuses[_statusName] += _statusAmount;
        }
    }
}