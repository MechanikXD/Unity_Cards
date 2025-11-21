using Core.Cards.Board;
using UnityEngine;

namespace Core.Cards.Card.Effects.Classes
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
                    context.OtherHand.TakeDamage(_damage);
                }
                else
                {
                    context.Other[context.Index - 1].Card.TakeDamage(_damage);
                }
            }

            if (context.Index + 1 < context.Other.Length)
            {
                if (context.Other[context.Index + 1].IsEmpty)
                {
                    context.OtherHand.TakeDamage(_damage);
                }
                else
                {
                    context.Other[context.Index + 1].Card.TakeDamage(_damage);
                }
            }
        }
    }
}