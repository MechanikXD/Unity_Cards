using Cards.Card.Data;
using Cards.Hand;
using Other.Extensions;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "Strength", menuName = "ScriptableObjects/Buff/Enemy/Strength")]
    public class Strength : EnemyBuff
    {
        [SerializeField] private int _targetCount;
        [SerializeField] private int _strengthCount;
        
        public override void Apply(PlayerData data)
        {
            var shuffled = data.Deck.ShuffledIndexes();
            
            for (var i = 0; i < Mathf.Min(_targetCount, shuffled.Length); i++)
            {
                data.ApplyBuffToCard(shuffled[i], Modify);
            }
        }

        protected override CardData Modify(CardData data)
        {
            var v2I = data.Attack;
            v2I.x += _strengthCount;
            v2I.y += _strengthCount;
            data.Attack = v2I;
            return data;
        }
    }
}