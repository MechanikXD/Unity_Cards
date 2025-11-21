using Other;
using UnityEngine;

namespace Core.Cards.Card.Data
{
    [CreateAssetMenu(fileName = "Card Data", menuName = "ScriptableObjects/Card DataBank")]
    public class CardDataBank : ScriptableObject
    {
        [SerializeField] private CardData[] _cards;

        public int Count => _cards.Length;
        public CardData Get(int index) => _cards[index];

        public int[] TakeRandom(int amount)
        {
            var indexes = new int[Count];

            for (var i = 0; i < Count; i++) indexes[i] = i;

            indexes.Shuffle();
            var random = new int[amount];
            for (var i = 0; i < amount; i++)
            {
                random[i] = indexes[i];
            }

            return random;
        }
    }
}