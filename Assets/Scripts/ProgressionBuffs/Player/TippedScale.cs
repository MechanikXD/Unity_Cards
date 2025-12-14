using Cards.Card.Effects;
using Cards.Hand;
using ProgressionBuffs.Scriptables;
using Structure.Managers;
using UnityEngine;

namespace ProgressionBuffs.Player
{
    [CreateAssetMenu(fileName = "TippedScale", menuName = "ScriptableObjects/Buff/Player/TippedScale")]
    public class TippedScale : PlayerBuff
    {
        private const string SIN_STATUS_KEY = "Sin";
        private const string HAS_STATUS_APPLY_EFFECT_KEY = "HasStatusApplyEffect";
        [SerializeField] private CardEffect _sinEffect;
        [SerializeField] private int _sinCap;
        
        public override void Apply(PlayerData data) => data.AddCombatStartEvent(DestroySin);

        private void DestroySin(PlayerData data)
        {
            var board = GameManager.Instance.Board;
            
            
            foreach (var enemySlot in board.EnemySlots)
            {
                if (!enemySlot.IsEmpty)
                {
                    if (enemySlot.Card.LocalStatuses.TryGetValue(SIN_STATUS_KEY, out var sinCount)
                        && sinCount >= _sinCap) enemySlot.Card.TakeDamage(999);
                }
                else
                {
                    if (!enemySlot.Card.LocalStatuses.ContainsKey(HAS_STATUS_APPLY_EFFECT_KEY))
                    {
                        enemySlot.Card.Data.AddEffect(TriggerType.OnHit, _sinEffect);
                        enemySlot.Card.LocalStatuses.Add(HAS_STATUS_APPLY_EFFECT_KEY, 0f);
                    }
                }
            }
            
            foreach (var playerSlot in board.PlayerSlots)
            {
                if (!playerSlot.IsEmpty)
                {
                    if (playerSlot.Card.LocalStatuses.TryGetValue(SIN_STATUS_KEY, out var sinCount)
                        && sinCount >= _sinCap) playerSlot.Card.TakeDamage(999);
                }
                else
                {
                    if (!playerSlot.Card.LocalStatuses.ContainsKey(HAS_STATUS_APPLY_EFFECT_KEY))
                    {
                        playerSlot.Card.Data.AddEffect(TriggerType.OnHit, _sinEffect);
                        playerSlot.Card.LocalStatuses.Add(HAS_STATUS_APPLY_EFFECT_KEY, 0f);
                    }
                }
            }
        }
    }
}