using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Player
{
    [CreateAssetMenu(fileName = "Health Up", menuName = "ScriptableObjects/Buff/Player/HealthUp")]
    public class HealthUp : PlayerBuff
    {
        [SerializeField] private int _healthBoost;
        
        public override void Apply(PlayerData data)
        {
            data.SetMaxHealth(data.MaxHealth + _healthBoost);
        }
    }
}