using Cards.Board;
using UnityEngine;

namespace Cards.Card.Effects.Classes
{
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
}