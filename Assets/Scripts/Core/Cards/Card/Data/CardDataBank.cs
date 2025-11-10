using UnityEngine;

namespace Core.Cards.Card.Data
{
    [CreateAssetMenu(fileName = "Card Data", menuName = "ScriptableObjects/Card DataBank")]
    public class CardDataBank : ScriptableObject
    {
        [SerializeField] private CardData[] _cards;

        public int Count => _cards.Length;
        public CardData Get(int index) => _cards[index];
    }
}