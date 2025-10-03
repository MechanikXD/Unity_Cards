using UnityEngine;

namespace Core.Cards.Card.Data
{
    public class CardDataBank : ScriptableObject
    {
        [SerializeField] private CardData[] _cards;

        public CardData Get(int index) => _cards[index];
    }
}