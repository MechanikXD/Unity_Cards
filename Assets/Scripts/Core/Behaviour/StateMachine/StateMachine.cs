using System;
using System.Collections.Generic;

namespace Core.Behaviour.StateMachine
{
    public class StateMachine
    {
        public State CurrentState { get; private set; }

        public virtual void Initialize(State initialState)
        {
            CurrentState = initialState;
            CurrentState.EnterState();
        }

        public void ChangeState(State newState)
        {
            CurrentState.ExitState();
            CurrentState = newState;
            CurrentState.EnterState();
        }

        public virtual void StopMachine()
        {
            CurrentState.ExitState();
            CurrentState = null;
        }
    }

    public sealed class StateMachine<TOwner> : StateMachine
    {
        private Dictionary<Type, State> _states;

        public override void Initialize(State initialState)
        {
            base.Initialize(initialState);
            _states = new Dictionary<Type, State> { { initialState.GetType(), initialState } };
        }

        public void AddState(State<TOwner> state) => _states.Add(state.GetType(), state);

        public void ChangeState<TState>() where TState : State<TOwner>
        {
            var newState = _states[typeof(TState)];
            ChangeState(newState);
        }

        public override void StopMachine()
        {
            base.StopMachine();
            _states.Clear();
        }
    }
}