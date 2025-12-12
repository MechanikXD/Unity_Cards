using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "Toughness", menuName = "ScriptableObjects/Buff/Enemy/Toughness")]
    public class Toughness : EnemyBuff
    {
        [SerializeField] private float _healthMultiply;
        
        public override void Apply(PlayerData data) => 
            data.SetMaxHealth((int)(data.MaxHealth * _healthMultiply));
    }
}