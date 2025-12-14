using Cards.Card.Effects;
using Cards.Hand;
using Other.Extensions;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Player
{
    [CreateAssetMenu(fileName = "AddEffects", menuName = "ScriptableObjects/Buff/Player/AddEffects")]
    public class AddEffects : PlayerBuff
    {
        [SerializeField] private CardEffect _effect;
        [SerializeField] private TriggerType _trigger;
        [SerializeField] private int _targetCount;
        
        public override void Apply(PlayerData data)
        {
            var shuffled = data.Deck.ShuffledIndexes();
            
            for (var i = 0; i < Mathf.Min(_targetCount, shuffled.Length); i++) 
                data.Deck[shuffled[i]].AddEffect(_trigger, _effect);
        }
    }
}