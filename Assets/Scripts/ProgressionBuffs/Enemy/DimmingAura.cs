using Cards.Hand;
using ProgressionBuffs.Scriptables;
using Structure.Managers;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "DimmingAura", menuName = "ScriptableObjects/Buff/Enemy/DimmingAura")]
    public class DimmingAura : EnemyBuff
    {
        [SerializeField] private int _playerCostIncrease;
        
        public override void Apply(PlayerData data)
        {
            var playerDeck = SessionManager.Instance.PlayerData.Deck;
            for (var i = 0; i < playerDeck.Length; i++) playerDeck[i].Cost += _playerCostIncrease;
        }
    }
}