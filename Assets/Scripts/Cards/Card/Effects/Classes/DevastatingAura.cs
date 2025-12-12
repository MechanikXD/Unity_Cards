using Cards.Board;
using UnityEngine;

namespace Cards.Card.Effects.Classes
{
    [CreateAssetMenu(fileName = "DevastatingAura", menuName = "ScriptableObjects/Card Effect/DevastatingAura")]
    public class DevastatingAura : CardEffect
    {
        [SerializeField] private int _damage;
        
        public override void Execute(BoardContext context)
        {
            foreach (var slot in context.Other)
            {
                if (!slot.IsEmpty) slot.Card.TakeDamage(_damage);
            }
        }
    }
}