using Core.Cards.Hand;
using UnityEngine;

namespace Player.Progression.Buffs.Player
{
    [CreateAssetMenu(fileName = "Health Up", menuName = "ScriptableObjects/Buff/Player/HealthUp")]
    public class HealthUp : PlayerBuff
    {
        [SerializeField] private int _healthBoost;
        
        public override void Apply(PlayerHand hand)
        {
            hand.SetMaxHealth(hand.MaxHealth + _healthBoost);
        }
    }
}