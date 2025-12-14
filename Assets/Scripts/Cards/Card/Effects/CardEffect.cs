using Cards.Board;
using UnityEngine;

namespace Cards.Card.Effects
{
    [CreateAssetMenu(fileName = "Card Effect", menuName = "ScriptableObjects/Card Effect/")]
    public abstract class CardEffect : ScriptableObject
    {
        [SerializeField] private string _description;
        [SerializeField] private bool _isHidden;
        public string Description => _description;
        public bool IsHidden => _isHidden;

        public abstract void Execute(BoardContext context);
    }
}