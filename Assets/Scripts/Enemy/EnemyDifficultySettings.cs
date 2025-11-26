using System;
using System.Collections.Generic;
using Core.Cards.Card;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    [CreateAssetMenu(fileName = "Difficulty Settings", menuName = "ScriptableObjects/Difficulty Settings")]
    public class EnemyDifficultySettings : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private int _minDeckSize;
        // Does not guarantee that deck will be that big, this is just a ceiling for deck size
        [SerializeField] private int _maxDeckSize = 12;
        [SerializeField] private DeckSelectionType _selectionType;
        [SerializeField] private int _deckMaxCost = 50;
        
        [Header("Aggressive")]
        // Value at which enemy AI will become aggressive
        [SerializeField] private float _dangerLevelToBecomeAggressive;
        // Any danger below given will be ignored, otherwise respond to it
        [SerializeField] private float _ignoreDangerLevelAggressive;
        [SerializeField] private bool _tryMatchDangerLevelOnCounteractAggressive;
        // If this enabled, Ai will try to use as less hope to put as given pressure on player
        [SerializeField] private bool _tryToPreserveHope;
        // Max level of danger that AI can put on player 
        [SerializeField] private float _maxPlayerPressureAggressive;
        [SerializeField] private float _dangerMultiplyFactorAggressive = 1f;
        
        [Header("Defensive")]
        // Value at which enemy will start playing defensively
        [SerializeField] private float _dangerLevelToBecomeDefensive;
        // If enabled, enemy will try to match player cards danger levels, otherwise - bodyblock player cards
        [SerializeField] private bool _tryToMatchDangerLevelsDefensive;
        // if danger level is above given, will respond immediately
        [SerializeField] private float _immediateRespondToDangerLevelDefensive;
        // How much hope to use per turn (_immediateRespondToDangerLevel can bypass it)
        [SerializeField] private float _maxHopeUsagePerTurnDefensive;
        [SerializeField] private float _dangerMultiplyFactorDefensive = 1f;
        
        [Header("Neutral")]
        // How many card to play per turn (at random)
        [SerializeField] private int _maxCardCountPerTurn;
        // If random selection is not on, will respond to card with given danger level
        [SerializeField] private float _minDangerLevelToRespondNeutral;
        
        // Aggressive state fields
        public float DangerLevelToBecomeAggressive => _dangerLevelToBecomeAggressive;
        public float IgnoreDangerLevelAggressive => _ignoreDangerLevelAggressive;
        public bool TryToPreserveHope => _tryToPreserveHope;
        public float MaxPlayerPressureAggressive => _maxPlayerPressureAggressive;
        public float DangerMultiplyFactorAggressive => _dangerMultiplyFactorAggressive;
        // Defensive state fields
        public float DangerLevelToBecomeDefensive => _dangerLevelToBecomeDefensive;
        public bool TryToMatchDangerLevelsDefensive => _tryToMatchDangerLevelsDefensive;
        public float ImmediateRespondToDangerLevelDefensive => _immediateRespondToDangerLevelDefensive;
        public float MaxHopeUsagePerTurnDefensive => _maxHopeUsagePerTurnDefensive;
        public float DangerMultiplyFactorDefensive => _dangerMultiplyFactorDefensive;
        // Neutral state fields 
        public int MaxCardCountPerTurn => _maxCardCountPerTurn;
        public float MinDangerLevelToRespondNeutral => _minDangerLevelToRespondNeutral;

        public int[] GetDeck()
        {
            return _selectionType switch
            {
                DeckSelectionType.Random => GetRandomDeck(),
                DeckSelectionType.Faction => GetRandomFaction(),
                DeckSelectionType.MaxHand => GetMaxHand(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private int[] GetRandomDeck() =>
            CardDataProvider.DataBank.TakeRandom(Random.Range(_minDeckSize, _maxDeckSize), _deckMaxCost);

        private int[] GetRandomFaction()
        {
            var db = CardDataProvider.DataBank;
            var randomFactionCard = db.Get(Random.Range(0, db.Count));
            var faction = randomFactionCard.Background;
            
            var currentCost = 0;
            var result = new List<int>();
            
            for (var i = 0; i < db.Count; i++)
            {
                var data = db.Get(i);
                if (data.Background != faction) continue;
                if (data.Cost + currentCost > _deckMaxCost) continue;
                
                result.Add(i);
                currentCost += data.Cost;
                if (currentCost == _deckMaxCost || result.Count >= _maxDeckSize) break;
            }

            return result.ToArray();
        }

        private int[] GetMaxHand()
        {
            var db = CardDataProvider.DataBank;
            
            var currentCost = 0;
            var result = new List<int>();
            
            for (var i = 0; i < db.Count; i++)
            {
                var data = db.Get(i);
                if (data.Cost > 3) continue;
                if (data.Cost + currentCost > _deckMaxCost) continue;
                
                result.Add(i);
                currentCost += data.Cost;
                if (currentCost == _deckMaxCost || result.Count >= _maxDeckSize) break;
            }

            if (currentCost - _deckMaxCost <= 3) return result.ToArray();

            for (var i = 0; i < db.Count; i++)
            {
                var data = db.Get(i);
                if (data.Cost > 6) continue;
                if (data.Cost + currentCost > _deckMaxCost) continue;
                
                result.Add(i);
                currentCost += data.Cost;
                if (currentCost - _deckMaxCost <= 3 || result.Count >= _maxDeckSize) break;
            }

            return result.ToArray();
        }
    }

    public enum DeckSelectionType
    {
        Random,
        Faction, // Based on card background
        MaxHand // Select cards with min cost
    }
}