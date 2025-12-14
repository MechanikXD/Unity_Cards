using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Player
{
    [CreateAssetMenu(fileName = "EverlastingLamp", menuName = "ScriptableObjects/Buff/Player/EverlastingLamp")]
    public class EverlastingLamp : PlayerBuff
    {
        [SerializeField] private int _cardDrawBoots;
        
        public override void Apply(PlayerData data)
        {
            data.SetLightRestore(999);
            data.SetCardDrawCount(data.DrawCount + _cardDrawBoots);
            data.AddCombatStartEvent(Lamp);
        }

        private void Lamp(PlayerData data)
        {
            if (data.CurrentLight > 0) data.TakeDamage(data.CurrentLight);
            if (data.CurrentDeck.Count == 0) data.RefillDeck();
        }
    }
}