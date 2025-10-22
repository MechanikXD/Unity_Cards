namespace Core.Behaviour.StateMachine
{
    public abstract class State
    {
        protected readonly StateMachine StateMachine;

        protected State(StateMachine sm)
        {
            StateMachine = sm;
        }
        
        public abstract void EnterState();
        public abstract void ExitState();
        
        public abstract void FrameUpdate();
        public abstract void FixedFrameUpdate();
    }

    public abstract class State<TOwner> : State
    {
        protected readonly TOwner StateOwner; 
        protected new readonly StateMachine<TOwner> StateMachine;

        protected State(StateMachine<TOwner> sm, TOwner owner) : base(sm)
        {
            StateOwner = owner;
            StateMachine = sm;
        }
    }
}