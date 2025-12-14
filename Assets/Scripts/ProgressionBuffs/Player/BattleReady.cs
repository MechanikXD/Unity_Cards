using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Player
{
    [CreateAssetMenu(fileName = "BattleReady", menuName = "ScriptableObjects/Buff/Player/BattleReady")]
    public class BattleReady : PlayerBuff
    {
        [SerializeField] private int _initialHandBoost;
        [SerializeField] private int _cardDrawBoost;
        
        public override void Apply(PlayerData data)
        {
            data.SetCardDrawCount(data.DrawCount + _cardDrawBoost);
            data.SetStartingHandSize(data.StartingHandSize + _initialHandBoost);
        }
    }
}