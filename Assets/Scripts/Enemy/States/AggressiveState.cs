using Core.Behaviour.StateMachine;
using Other;

namespace Enemy.States
{
    public class AggressiveState : EnemyState
    {
        public AggressiveState(StateMachine<EnemyBehaviour> sm, EnemyBehaviour owner) : base(sm, owner) { }

        protected override float GetCurrentStateDangerMultiplier() =>
            StateOwner.Settings.DangerMultiplyFactorAggressive;

        public override void PlayTurn()
        {
            var currentDangerLevels = GetDangerLevels();

            var dangerLevelsOnPlayer = new float[Board.PlayerSlots.Length];
            var totalPressure = 0f;
            // Only when card slot can attack player directly (player has no card) it is 0
            // Otherwise there will be some kind of value.
            for (var i = 0; i < dangerLevelsOnPlayer.Length; i++)
            {
                var playerCard = Board.PlayerSlots[i];
                var myCard = Board.EnemySlots[i];
                
                switch (playerCard.IsEmpty)
                {
                    // both empty
                    case true when myCard.IsEmpty:
                        dangerLevelsOnPlayer[i] = 0f;
                        break;
                    // player card only
                    case false when myCard.IsEmpty:
                        dangerLevelsOnPlayer[i] = -playerCard.Card.CardData.Attack.Average();
                        break;
                    // My card only
                    case true when !myCard.IsEmpty:
                        var attackValue = myCard.Card.CardData.Attack.Average();
                        dangerLevelsOnPlayer[i] = attackValue;
                        totalPressure += attackValue;
                        break;
                    // Both slots have a card.
                    default:
                        dangerLevelsOnPlayer[i] = 1f;
                        break;
                }
            }
            
            // If there are too dangerous slots (Settings.IgnoreDangerLevelAggressive) counteract
            // If there are slots where can directly attack player AND player pressure is too low
            //      -> play card (check Settings.TryToPreserveHope boolean)
            // Then end turn.
        }
        
        public override void EnterState() { }
        public override void ExitState() { }
        public override void FrameUpdate() { }
        public override void FixedFrameUpdate() { }
    }
}