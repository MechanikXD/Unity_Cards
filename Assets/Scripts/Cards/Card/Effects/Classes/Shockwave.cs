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
}