using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "Survivability", menuName = "ScriptableObjects/Buff/Enemy/Survivability")]
    public class Survivability : EnemyBuff
    {
        [SerializeField] private int _playerHealthBoost;
        [SerializeField] private int _healthRegeneration;
        
        public override void Apply(PlayerData data)
        {
            data.SetMaxHealth(data.MaxHealth + _playerHealthBoost);
            data.AddCombatStartEvent(RegeneratePLayerHealth);
        }

        private void RegeneratePLayerHealth(PlayerData data) => 
            data.RestoreHealth(_healthRegeneration);
    }
}