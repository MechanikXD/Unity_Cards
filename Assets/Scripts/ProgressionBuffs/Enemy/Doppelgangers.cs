using Cards.Card;
using Cards.Hand;
using Other.Extensions;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "Doppelgangers", menuName = "ScriptableObjects/Buff/Enemy/Doppelgangers")]
    public class Doppelgangers : EnemyBuff
    {
        [SerializeField] private int _cardDuplicateCount;
        
        public override void Apply(PlayerData data)
        {
            var db = CardDataProvider.DataBank;
            var shuffled = data.Deck.ShuffledIndexes();
            
            for (var i = 0; i < Mathf.Min(_cardDuplicateCount, shuffled.Length); i++)
            {
                data.CurrentDeck.AddLast(db.Get(shuffled[i]));
                data.ApplyBuffToCard(shuffled[i], Modify);
            }
        }
    }
}