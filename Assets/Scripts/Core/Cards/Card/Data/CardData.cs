using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cards.Card.Effects;
using UnityEngine;

namespace Core.Cards.Card.Data
{
    [Serializable]
    public struct CardData : IEquatable<CardData>
    {
        [SerializeField] private int _id;
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

        public int ID => _id;

        public Sprite Sprite => _sprite;
        public Sprite Background => _background;
        public CardAffinity Affinity => _affinity;
        
        public int Health =>  _health;
        public Vector2Int Attack => _attack;
        public int Cost => _cost;

        public Dictionary<TriggerType, CardEffect[]> Effects
        {
            get
            {
                return _effects != null
                    ? _effects.ToDictionary(e => e.Trigger, e => e.Effects)
                    : new Dictionary<TriggerType, CardEffect[]>();
            }
        }
        
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
            return _id == other._id && _affinity == other._affinity && _health == other._health &&
                   _attack.Equals(other._attack) && _cost == other._cost;
        }

        public override bool Equals(object obj)
        {
            return obj is CardData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_id, (int)_affinity, _health, _attack, _cost);
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