using Core.Behaviour.StateMachine;
using Other;

namespace Enemy.States
{
    public class DefensiveState : EnemyState
    {
        public DefensiveState(StateMachine<EnemyBehaviour> sm, EnemyBehaviour owner) : base(sm, owner) { }

        protected override float GetCurrentStateDangerMultiplier() =>
            StateOwner.Settings.DangerMultiplyFactorDefensive;

        public override void PlayTurn()
        {
            var currentDangerLevels = GetDangerLevels();

            var dangerLevelsOnMe = new float[Board.PlayerSlots.Length];
            // Danger level will appear only if player card can directly attack enemy
            for (var i = 0; i < dangerLevelsOnMe.Length; i++)
            {
                var playerCard = Board.PlayerSlots[i];
                var myCard = Board.EnemySlots[i];
                
                switch (playerCard.IsEmpty)
                {
                    // both empty
                    case true when myCard.IsEmpty:
                        dangerLevelsOnMe[i] = 0f;
                        break;
                    // player card only
                    case false when myCard.IsEmpty:
                        dangerLevelsOnMe[i] = playerCard.Card.CardData.Attack.Average();
                        break;
                    // My card only OR Both slots have a card.
                    case true when !myCard.IsEmpty:
                    default:
                        dangerLevelsOnMe[i] = -1f;
                        break;
                }
            }
            
            // Respond to slots with max danger level (MaxHopeUsagePerTurnDefensive, TryToMatchDangerLevelsDefensive)
            // If all slots are defended -> switch state or skip turn.
        }
        
        public override void EnterState() { }
        public override void ExitState() { }
        public override void FrameUpdate() { }
        public override void FixedFrameUpdate() { }
    }
}