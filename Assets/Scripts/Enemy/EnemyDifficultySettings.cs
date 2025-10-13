using UnityEngine;

namespace Enemy
{
    public class EnemyDifficultySettings : ScriptableObject
    {
        [Header("Aggressive")]
        // Value at which enemy AI will become aggressive
        [SerializeField] private float _dangerLevelToBecomeAggressive;
        // Any danger below given will be ignored, otherwise respond to it
        [SerializeField] private float _ignoreDangerLevelAggressive;
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
        // How to pick card slot (if selected -> random, otherwise with more profit:
        // how much damage can deal to how much will receive)
        // Card selected on random.
        [SerializeField] private bool _randomSlotSelection;
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
        public bool RandomSlotSelection => _randomSlotSelection;
        public float MinDangerLevelToRespondNeutral => _minDangerLevelToRespondNeutral;
    }
}