using System;
using System.Collections.Generic;
using Cards.Card;
using Cards.Card.Data;
using Cards.Card.Effects;
using Cards.Hand;
using Other.Extensions;
using Structure.Managers;
using UnityEngine;

namespace ProgressionBuffs.Player
{
    [CreateAssetMenu(fileName = "Health Up", menuName = "ScriptableObjects/Buff/Player/HealthUp")]
    public class HealthUp : PlayerBuff
    {
        [SerializeField] private int _healthBoost;
        
        public override void Apply(PlayerData data)
        {
            data.SetMaxHealth(data.MaxHealth + _healthBoost);
        }
    }

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

    [CreateAssetMenu(fileName = "Synergy", menuName = "ScriptableObjects/Buff/Player/Synergy")]
    public class Synergy : PlayerBuff
    {
        [SerializeField] private float _multiply;
        
        public override void Apply(PlayerData data)
        {
            var dict = new Dictionary<CardAffinity, int>();
            foreach (var affinity in Enum.GetValues(typeof(CardAffinity))) 
                dict.Add((CardAffinity)affinity, 0);
            // Count affinities
            foreach (var card in data.Deck) dict[card.Affinity]++;
            // Apply strength
            for (var i = 0; i < data.Deck.Length; i++) 
                data.Deck[i] = ModifyAttack(data.Deck[i], 
                    (int)(dict[data.Deck[i].Affinity] * _multiply));
        }

        private static CardData ModifyAttack(CardData data, int attack)
        {
            var v2I = data.Attack;
            v2I.x += attack;
            v2I.y += attack;
            data.Attack = v2I;
            return data;
        }
    }

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