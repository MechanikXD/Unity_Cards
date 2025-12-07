using System;
using System.Collections.Generic;
using Cards.Card.Effects;
using SaveLoad.Serializeables;
using UnityEngine;

namespace Cards.Card.Data
{
    [Serializable]
    public struct CardData : IEquatable<CardData>
    {
        [Header("Visual")]
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Sprite _background;
        [SerializeField] private CardAffinity _affinity;
        [Header("Stats")]
        [SerializeField] private int _health;
        [SerializeField] private Vector2Int _attack;
        [SerializeField] private int _cost;
        [Header("Effects")]
        [SerializeField] private EffectGroup[] _effects;

        public int ID { get; internal set; }

        public Sprite Sprite => _sprite;
        public Sprite Background => _background;
        public CardAffinity Affinity => _affinity;
        
        public int Health
        {
            get => _health;
            set => _health = value;
        }

        public Vector2Int Attack
        {
            get => _attack;
            set => _attack = value;
        }

        public int Cost
        {
            get => _cost;
            set => _cost = value;
        }

        private Dictionary<TriggerType, List<CardEffect>> _effectDict;
        public Dictionary<TriggerType, List<CardEffect>> Effects
        {
            get
            {
                if (_effectDict == null) ParseEffects();
                return _effectDict;
            }
        }

        public void AddEffect(TriggerType trigger, CardEffect effect) => Effects[trigger].Add(effect);
        public void RemoveEffect(TriggerType trigger, CardEffect effect) => Effects[trigger].Remove(effect);

        public static bool operator ==(CardData thisCard, CardData otherCard)
        {
            return thisCard.Equals(otherCard);
        }

        public static bool operator !=(CardData thisCard, CardData otherCard)
        {
            return !(thisCard == otherCard);
        }

        public bool Equals(CardData other)
        {
            return ID == other.ID && _affinity == other._affinity && _health == other._health &&
                   _attack.Equals(other._attack) && _cost == other._cost;
        }

        public override bool Equals(object obj)
        {
            return obj is CardData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, (int)_affinity, _health, _attack, _cost);
        }

        private void ParseEffects()
        {
            _effectDict = new Dictionary<TriggerType, List<CardEffect>>();
            foreach (var effectGroup in _effects)
            {
                if (!_effectDict.ContainsKey(effectGroup.Trigger))
                {
                    _effectDict.Add(effectGroup.Trigger, new List<CardEffect>());
                }    
                
                _effectDict[effectGroup.Trigger].AddRange(effectGroup.Effects);
            }
            
        }

        public SerializableCardData SerializeSelf()
        {
            return new SerializableCardData(ID, Health, Attack, Cost);
        }
    }

    [Serializable]
    public struct EffectGroup
    {
        [SerializeField] private TriggerType _trigger;
        [SerializeField] private CardEffect[] _effects;
        
        public TriggerType Trigger => _trigger;
        public CardEffect[] Effects => _effects;
    }
}