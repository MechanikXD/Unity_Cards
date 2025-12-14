using System;
using System.Collections.Generic;
using Cards.Card;
using Cards.Card.Data;
using Cards.Hand;
using ProgressionBuffs.Scriptables;
using UnityEngine;

namespace ProgressionBuffs.Player
{
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
}