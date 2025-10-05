using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cards.Card.Effects;
using UnityEngine;

namespace Core.Cards.Card.Data
{
    [Serializable]
    public struct CardData
    {
        [SerializeField] private int _id;
        [Header("Visual")]
        [SerializeField] private Sprite _image;
        [SerializeField] private CardAffinity _affinity;
        [Header("Stats")]
        [SerializeField] private int _health;
        [SerializeField] private Vector2Int _attack;
        [SerializeField] private int _cost;
        [Header("Effects")]
        [SerializeField] private EffectGroup[] _effects;

        public int ID => _id;

        public Sprite Image => _image;
        public CardAffinity Affinity => _affinity;
        
        public int Health =>  _health;
        public Vector2Int Attack => _attack;
        public int Cost => _cost;

        public Dictionary<TriggerType, CardEffect[]> Effects =>
            _effects.ToDictionary(e => e.Trigger, e => e.Effects);
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