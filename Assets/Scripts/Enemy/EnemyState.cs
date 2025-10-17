using System.Linq;
using Core.Behaviour.StateMachine;
using Core.Cards.Board;
using Core.Cards.Card.Data;
using Core.Cards.Hand;
using Enemy.States;
using Other;
using UnityEngine;

namespace Enemy
{
    public abstract class EnemyState : State<EnemyBehaviour>
    {
        // For quick access
        protected readonly BoardModel Board;
        protected readonly PlayerHand Hand;

        protected EnemyState(StateMachine<EnemyBehaviour> sm, EnemyBehaviour owner) : base(sm, owner)
        {
            Board = owner.Board;
            Hand = owner.Hand;
        }

        protected abstract float GetCurrentStateDangerMultiplier();
        
        public abstract void PlayTurn();

        protected float[] GetDangerLevels()
        {
            var myCards = Board.EnemySlots;
            var playerCards = Board.PlayerSlots;
            var dangerLevels = new float[myCards.Length];

            for (var i = 0; i < myCards.Length; i++)
            {
                var playerDangerLevel = playerCards[i].IsEmpty ? 0 : playerCards[i].Card.CardData.Attack.Average();
                var myDangerLevel = myCards[i].IsEmpty ?  0 : myCards[i].Card.CardData.Attack.Average();
                var difference = playerDangerLevel - myDangerLevel;
                
                if (myDangerLevel == 0 && Hand.CurrentHealth - playerDangerLevel <= 0)
                {
                    dangerLevels[i] = float.MaxValue;
                }
                else
                {
                    dangerLevels[i] = difference * GetCurrentStateDangerMultiplier();
                }
            }
            
            return dangerLevels;
        }
        
        protected CardData? GetCardWithLeastCost()
        {
            var hand = Hand.CardsInHand;
            var leastHopeCost = int.MaxValue;
            var leastHopeCostIndex = -1;
            
            for (var i = 0; i < hand.Count; i++)
            {
                if (hand[i].Cost >= leastHopeCost) continue;

                leastHopeCost = hand[i].Cost;
                leastHopeCostIndex = i;
            }

            if (leastHopeCostIndex != -1 && Hand.CanUseCard(leastHopeCost)) 
                return Hand.GetCardFromHand(leastHopeCostIndex);
            else return null;
        }

        protected CardData? GetCardWithMatchingCost(float match)
        {
            var hand = Hand.CardsInHand;
            var closestDangerDiff = float.MaxValue;
            var closestDangerIndex = -1;
            
            for (var i = 0; i < hand.Count; i++)
            {
                if (!Hand.CanUseCard(hand[i].Cost)) continue;
                
                var attack = hand[i].Attack.Average();
                var difference = Mathf.Abs(match - attack);
                
                if (difference >= closestDangerDiff) continue;

                closestDangerDiff = difference;
                closestDangerIndex = i;
            }

            if (closestDangerIndex != -1) return Hand.GetCardFromHand(closestDangerIndex);
            else return null;
        }
        
        public void AutoChangeState() => AutoChangeState(GetDangerLevels().Sum());

        public void AutoChangeState(float dangerLevel)
        {
            if (dangerLevel <= StateOwner.Settings.DangerLevelToBecomeAggressive)
            {
                StateMachine.ChangeState<AggressiveState>();
            }
            else if (dangerLevel >= StateOwner.Settings.DangerLevelToBecomeDefensive)
            {
                StateMachine.ChangeState<DefensiveState>();
            }
            else
            {
                StateMachine.ChangeState<NeutralState>();
            }
        }

        public sealed override void FrameUpdate() {}
        public sealed override void FixedFrameUpdate() {}
    }
}