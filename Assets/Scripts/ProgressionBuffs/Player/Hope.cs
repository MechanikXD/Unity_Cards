using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Player
{
    [CreateAssetMenu(fileName = "Hope", menuName = "ScriptableObjects/Buff/Player/Hope")]
    public class Hope : PlayerBuff
    {
        [SerializeField] private int _lightBoost;
        [SerializeField] private int _lightRegenerationBoost;
        
        public override void Apply(PlayerData data)
        {
            data.SetMaxLight(data.MaxLight + _lightBoost);
            data.SetLightRestore(data.LightRegeneration + _lightRegenerationBoost);
        }
    }
}