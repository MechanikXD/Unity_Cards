using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "HastyDraw", menuName = "ScriptableObjects/Buff/Enemy/HastyDraw")]
    public class HastyDraw : EnemyBuff
    {
        [SerializeField] private int _drawCountBonus;
        
        public override void Apply(PlayerData data)
        {
            data.SetCardDrawCount(data.DrawCount + _drawCountBonus);
        }
    }
}