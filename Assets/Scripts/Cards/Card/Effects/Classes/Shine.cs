using Cards.Board;
using UnityEngine;

namespace Cards.Card.Effects.Classes
{
    [CreateAssetMenu(fileName = "Shine", menuName = "ScriptableObjects/Card Effect/Shine")]
    public class Shine : CardEffect
    {
        [SerializeField] private int _lightRestore;
        
        public override void Execute(BoardContext context) => 
            context.PlayerData.AddLight(_lightRestore);
    }
}