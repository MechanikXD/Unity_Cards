using Core.Cards.Board;
using UnityEngine;

namespace Core.Cards.Card.Effects
{
    [CreateAssetMenu(fileName = "Card Effect", menuName = "ScriptableObjects/Card Effect/")]
    public abstract class CardEffect : ScriptableObject
    {
        [SerializeField] private string _description;
        public string Description => _description;

        public abstract void Execute(BoardContext context);
    }
}