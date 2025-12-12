using Cards.Board;
using UnityEngine;

namespace Cards.Card.Effects.Classes
{
    [CreateAssetMenu(fileName = "Berserk", menuName = "ScriptableObjects/Card Effect/Berserk")]
    public class Berserk : CardEffect
    {
        [SerializeField] private float _healthThreshold;
        [SerializeField] private int _bonusDamage;
        
        public override void Execute(BoardContext context)
        {
            var slot = context.Player[context.Index];
            if (!slot.IsEmpty && slot.Card.CurrentHealth <= slot.Card.Data.Health * _healthThreshold)
            {
                var othersSlot = context.Other[context.Index];
                if (!othersSlot.IsEmpty) othersSlot.Card.TakeDamage(_bonusDamage);
                else context.OtherData.TakeDamage(_bonusDamage);
            }
        }
    }
}