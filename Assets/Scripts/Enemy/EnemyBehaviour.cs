using System.Collections.Generic;
using Cards.Board;
using Cards.Card.Data;
using Cards.Hand;
using Enemy.States;
using Structure.StateMachine;

namespace Enemy
{
    public class EnemyBehaviour
    {
        public StateMachine<EnemyBehaviour> StateMachine { get; private set; }
        public PlayerData Data { get; private set; }
        public BoardModel Board { get; private set; }
        public EnemyDifficultySettings Settings { get; private set; }

        private readonly List<(CardData data, int index)> _inputBuffer;

        public EnemyBehaviour(BoardModel board, PlayerData data, EnemyDifficultySettings settings)
        {
            _inputBuffer =  new List<(CardData data, int index)>();
            Board = board;
            Settings = settings;
            Data = data;
            StateMachine = new StateMachine<EnemyBehaviour>();
            var aggressiveState = new AggressiveState(StateMachine, this);
            var defensiveState = new DefensiveState(StateMachine, this);
            var neutralState = new NeutralState(StateMachine, this);
            StateMachine.Initialize(neutralState);
            StateMachine.AddState(defensiveState);
            StateMachine.AddState(aggressiveState);
        }

        public void PlayTurn()
        {
            if (Data.CardsInHand.Count == 0) return;
            ((EnemyState)StateMachine.CurrentState).PlayTurn();
        }

        public void PlayCard(CardData card, int slotIndex)
        {
            if (!Data.CanUseCard(card.Cost)) return;
            
            _inputBuffer.Add((card, slotIndex));
            Data.UseLight(card.Cost);
        }

        public void FinishTurn()
        {
            foreach (var input in _inputBuffer) 
                Board.PlaceEnemyCard(input.data, input.index);
            
            ((EnemyState)StateMachine.CurrentState).AutoChangeState();
            _inputBuffer.Clear();
        }
    }
}