using Cards.Card.Data;
using Cards.Hand;
using Other.Extensions;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "ThickSkin", menuName = "ScriptableObjects/Buff/Enemy/ThickSkin")]
    public class ThickSkin : EnemyBuff
    {
        [SerializeField] private int _targetCount;
        [SerializeField] private int _healthBoost;
        
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
            data.Health += _healthBoost;
            return data;
        }
    }
}