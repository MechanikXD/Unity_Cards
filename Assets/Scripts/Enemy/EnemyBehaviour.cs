using System.Collections.Generic;
using Cards.Board;
using Cards.Card;
using Cards.Card.Data;
using Cards.Hand;
using Enemy.States;
using Structure.StateMachine;
using UnityEngine;

namespace Enemy
{
    public class EnemyBehaviour
    {
        public StateMachine<EnemyBehaviour> StateMachine { get; private set; }
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
            if (Hand.CardsInHand.Count == 0) return;
            ((EnemyState)StateMachine.CurrentState).PlayTurn();
        }

        public void PlayCard(CardData card, int slotIndex)
        {
            if (!Hand.CanUseCard(card.Cost)) return;
            
            _inputBuffer.Add((card, slotIndex));
            Hand.UseHope(card.Cost);
        }

        public void FinishTurn()
        {
            foreach (var input in _inputBuffer)
            {
                var thisSlot = Board.EnemySlots[input.index];
                if (!thisSlot.IsEmpty) continue;

                var newCard = Object.Instantiate(Board.CardPrefab);
                
                thisSlot.Attach(newCard);
                newCard.Animator.enabled = true;
                newCard.GetComponent<CardController>().Interactable = false;
                newCard.Set(input.data, null);
            }
            
            ((EnemyState)StateMachine.CurrentState).AutoChangeState();
            _inputBuffer.Clear();
        }
    }
}