using System;
using Core.Behaviour.StateMachine;
using Core.Cards.Board;
using Core.Cards.Card.Data;
using Core.Cards.Hand;
using Enemy.States;
using UnityEngine;

namespace Enemy
{
    public class EnemyBehaviour : MonoBehaviour
    {
        private StateMachine<EnemyBehaviour> _stateMachine;
        private BoardModel _board;
        private PlayerHand _enemyHand;
        private EnemyDifficultySettings _difficultySettings;

        public PlayerHand Hand => _enemyHand;
        public BoardModel Board => _board;
        public EnemyDifficultySettings Settings => _difficultySettings; 

        public static event Action EnemyTurnFinished;

        public void Initialize(PlayerHand hand)
        {
            _enemyHand = hand;
            _stateMachine = new StateMachine<EnemyBehaviour>();
            var aggressiveState = new AggressiveState(_stateMachine, this);
            var defensiveState = new DefensiveState(_stateMachine, this);
            var neutralState = new NeutralState(_stateMachine, this);
            _stateMachine.Initialize(neutralState);
            _stateMachine.AddState(defensiveState);
            _stateMachine.AddState(aggressiveState);
        }

        public void PlayCard(CardData card, int slotIndex)
        {
            throw new NotImplementedException();
        }
        
        public void FinishTurn() => EnemyTurnFinished?.Invoke();

        /*private void Update()
        {
            _stateMachine.CurrentState.FrameUpdate();
        }
        
        private void FixedUpdate()
        {
            _stateMachine.CurrentState.FixedFrameUpdate();
        }*/
    }
}