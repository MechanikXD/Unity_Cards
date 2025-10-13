using Core.Behaviour.StateMachine;

namespace Enemy.States
{
    public class NeutralState : EnemyState
    {
        public NeutralState(StateMachine<EnemyBehaviour> sm, EnemyBehaviour owner) : base(sm, owner) { }

        protected override float GetCurrentStateDangerMultiplier() => 1f;

        public override void PlayTurn()
        {
            var rng = new System.Random();
            var cardsToPlay = rng.Next(StateOwner.Settings.MaxCardCountPerTurn + 1);
            
            // if (RandomSlotSelection) select random card and play it
            // else respond to dangerous cards and play others if (my card damage > player card's
            //   => try to find other slot. else play card there. if no free slots play on lowest difference)
        }
        
        public override void EnterState() { }
        public override void ExitState() { }
        public override void FrameUpdate() { }
        public override void FixedFrameUpdate() { }
    }
}