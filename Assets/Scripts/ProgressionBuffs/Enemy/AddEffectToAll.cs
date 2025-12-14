using Cards.Card.Effects;
using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Enemy
{
    [CreateAssetMenu(fileName = "AddEffectToAll", menuName = "ScriptableObjects/Buff/Enemy/AddEffectToAll")]
    public class AddEffectToAll : EnemyBuff
    {
        [SerializeField] private CardEffect _effect;
        [SerializeField] private TriggerType _trigger;
        
        public override void Apply(PlayerData data)
        {
            foreach (var card in data.Deck)
            {
                if (card.ContainsEffect(_trigger, _effect)) continue;
                card.AddEffect(_trigger, _effect);
            }
        }
    }
}