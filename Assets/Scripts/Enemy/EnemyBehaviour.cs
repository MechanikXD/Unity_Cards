using System.Collections.Generic;
using Core.Behaviour.StateMachine;
using Core.Cards.Board;
using Core.Cards.Card.Data;
using Core.Cards.Hand;
using Enemy.States;
using Player;
using UnityEngine;

namespace Enemy
{
    public class EnemyBehaviour
    {
        private readonly StateMachine<EnemyBehaviour> _stateMachine;
        public PlayerHand Hand { get; private set; }
        public BoardModel Board { get; private set; }
        public EnemyDifficultySettings Settings { get; private set; }

        private readonly List<(CardData data, int index)> _inputBuffer;

        public EnemyBehaviour(BoardModel board, PlayerHand hand, EnemyDifficultySettings settings)
        {
            _inputBuffer =  new List<(CardData data, int index)>();
            Board = board;
            Settings = settings;
            Hand = hand;
            _stateMachine = new StateMachine<EnemyBehaviour>();
            var aggressiveState = new AggressiveState(_stateMachine, this);
            var defensiveState = new DefensiveState(_stateMachine, this);
            var neutralState = new NeutralState(_stateMachine, this);
            _stateMachine.Initialize(neutralState);
            _stateMachine.AddState(defensiveState);
            _stateMachine.AddState(aggressiveState);
        }

        public void PlayTurn()
        {
            ((EnemyState)_stateMachine.CurrentState).PlayTurn();
        }

        public void PlayCard(CardData card, int slotIndex)
        {
            if (!Hand.CanUseCard(card.Cost)) return;
            
            _inputBuffer.Add((card, slotIndex));
            Hand.UseHope(card.Cost);
        }

        public void FinishTurn()
        {
            Debug.Log($"Cards to use: {_inputBuffer.Count}");
            foreach (var input in _inputBuffer)
            {
                Debug.Log($"Input: {input.data.Sprite.name}, {input.index}");
                var thisSlot = Board.EnemySlots[input.index];
                if (!thisSlot.IsEmpty) continue;

                Debug.Log("Slot was empty...");
                var newCard = Object.Instantiate(Board.CardPrefab);
                
                thisSlot.Attach(newCard);
                newCard.EnableAnimator();
                Object.Destroy(newCard.GetComponent<CardDragHandler>());
                newCard.Set(input.data, null);
            }
            
            ((EnemyState)_stateMachine.CurrentState).AutoChangeState();
            _inputBuffer.Clear();
        }
    }
}