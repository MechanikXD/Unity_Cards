using Cards.Board;
using UnityEngine;

namespace Cards.Card.Effects.Classes
{
    [CreateAssetMenu(fileName = "Shockwave", menuName = "ScriptableObjects/Card Effect/Shockwave")]
    public class Shockwave : CardEffect
    {
        [SerializeField] private int _damage = 1;
        
        public override void Execute(BoardContext context)
        {
            if (context.Index - 1 > 0)
            {
                if (context.Other[context.Index - 1].IsEmpty)
                {
                    context.OtherData.TakeDamage(_damage);
                }
                else
                {
                    var card = context.Other[context.Index - 1].Card;
                    card.TakeDamage(_damage);
                    if (card.IsDefeated)
                    {
                        context.Other[context.Index - 1].Detach();
                        Destroy(card.gameObject);
                    }
                }
            }

            if (context.Index + 1 < context.Other.Length)
            {
                if (context.Other[context.Index + 1].IsEmpty)
                {
                    context.OtherData.TakeDamage(_damage);
                }
                else
                {
                    var card = context.Other[context.Index + 1].Card;
                    card.TakeDamage(_damage);
                    if (card.IsDefeated)
                    {
                        context.Other[context.Index + 1].Detach();
                        Destroy(card.gameObject);
                    }
                }
            }
        }
    }

    [CreateAssetMenu(fileName = "Darkness", menuName = "ScriptableObjects/Card Effect/Darkness")]
    public class Darkness : CardEffect
    {
        public override void Execute(BoardContext context)
        {
            if (context.Other[context.Index].IsEmpty) context.OtherData.UseLight(1);
        }
    }

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

    [CreateAssetMenu(fileName = "Punish", menuName = "ScriptableObjects/Card Effect/Punish")]
    public class Punish : CardEffect
    {
        [SerializeField] private string _statusName = "Punishment";
        [SerializeField] private int _bonusDamage;
        
        public override void Execute(BoardContext context)
        {
            var slot = context.Other[context.Index];
            if (!slot.IsEmpty && slot.Card.LocalStatuses.ContainsKey(_statusName))
            {
                slot.Card.TakeDamage(_bonusDamage);
            }
        }
    }
    
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

    [CreateAssetMenu(fileName = "LightShatter", menuName = "ScriptableObjects/Card Effect/LightShatter")]
    public class LightShatter : CardEffect
    {
        [SerializeField] private int _lightDrain;
        
        public override void Execute(BoardContext context)
        {
            if (context.Other[context.Index].IsEmpty)
                context.OtherData.UseLight(_lightDrain);
        }
    }

    [CreateAssetMenu(fileName = "Hunger", menuName = "ScriptableObjects/Card Effect/Hunger")]
    public class Hunger : CardEffect
    {
        public override void Execute(BoardContext context)
        {
            if (context.Other[context.Index].IsEmpty && context.OtherData.CardsInHand.Count > 0)
            {
                var first = context.OtherData.CardsInHand[0];
                context.OtherData.CardsInHand.RemoveAt(0);
                context.OtherData.CurrentDeck.AddLast(first);
            }
        }
    }

    [CreateAssetMenu(fileName = "Shine", menuName = "ScriptableObjects/Card Effect/Shine")]
    public class Shine : CardEffect
    {
        [SerializeField] private int _lightRestore;
        
        public override void Execute(BoardContext context) => 
            context.PlayerData.AddLight(_lightRestore);
    }
}