using System.Collections.Generic;
using Other.Extensions;
using UnityEngine;

namespace Cards.Card.Data
{
    [CreateAssetMenu(fileName = "Card Data", menuName = "ScriptableObjects/Card DataBank")]
    public class CardDataBank : ScriptableObject
    {
        [SerializeField] private CardData[] _cards;

        public int Count => _cards.Length;
        public CardData Get(int index)
        {
            var card = _cards[index];
            card.ID = index;
            return card;
        }

        public int[] TakeRandom(int amount, int maxCost, bool tryFillDeck=false)
        {
            var indexes = new int[Count];

            for (var i = 0; i < Count; i++) indexes[i] = i;

            indexes.Shuffle();
            var currentCost = 0;
            var random = new List<int>(amount);
            
            for (var i = 0; i < amount; i++)
            {
                var data = Get(indexes[i]);
                if (data.Cost + currentCost > maxCost)
                {
                    if (!tryFillDeck) break;
                    continue;
                }
                
                random.Add(indexes[i]);
                currentCost += data.Cost;
                if (currentCost == maxCost) break;
            }

            return random.ToArray();
        }
    }
}