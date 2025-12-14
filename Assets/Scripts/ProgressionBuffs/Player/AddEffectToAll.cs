using Cards.Card.Effects;
using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Player
{
    [CreateAssetMenu(fileName = "AddEffectToAll", menuName = "ScriptableObjects/Buff/Player/AddEffectToAll")]
    public class AddEffectToAll : PlayerBuff
    {
        [SerializeField] private CardEffect _effect;
        [SerializeField] private TriggerType _trigger;
        
        public override void Apply(PlayerData data)
        {
            foreach (var card in data.Deck) card.AddEffect(_trigger, _effect);
        }
    }
}