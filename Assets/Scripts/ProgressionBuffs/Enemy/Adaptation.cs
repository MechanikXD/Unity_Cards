using Cards.Hand;
using Other.Extensions;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "Adaptation", menuName = "ScriptableObjects/Buff/Enemy/Adaptation")]
    public class Adaptation : EnemyBuff
    {
        [SerializeField] private int _strongestHealthBoost;
        [SerializeField] private int _weakestAttackBoost;
        
        public override void Apply(PlayerData data)
        {
            var weakestIndex = 0;
            var weakestAttack = float.MaxValue;
            var strongestIndex = 0;
            var strongestAttack = -1.0;
            
            for (var i = 0; i < data.Deck.Length; i++)
            {
                var average = data.Deck[i].Attack.Average();
                if (average < weakestAttack)
                {
                    weakestAttack = average;
                    weakestIndex = i;
                }
                if (average > strongestAttack)
                {
                    strongestAttack = average;
                    strongestIndex = i;
                }
            }

            data.Deck[strongestIndex].Health += _strongestHealthBoost;
            var v2I = data.Deck[weakestIndex].Attack;
            v2I.x += _weakestAttackBoost;
            v2I.y += _weakestAttackBoost;
            data.Deck[weakestIndex].Attack = v2I;
        }
    }
}