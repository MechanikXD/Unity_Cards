using System.Collections.Generic;
using Core.Behaviour.StateMachine;
using Core.Cards.Card.Data;
using Other;
using UnityEngine;

namespace Enemy.States
{
    public class DefensiveState : EnemyState
    {
        public DefensiveState(StateMachine<EnemyBehaviour> sm, EnemyBehaviour owner) : base(sm, owner) { }

        protected override float GetCurrentStateDangerMultiplier() =>
            StateOwner.Settings.DangerMultiplyFactorDefensive;

        public override void PlayTurn()
        {
            var otherDangerSlots = new List<int>();
            var immediateRespondSlots = new List<int>();
            // Danger level will appear only if player card can directly attack enemy
            for (var i = 0; i < Board.EnemySlots.Length; i++)
            {
                var playerCard = Board.PlayerSlots[i];
                var myCard = Board.EnemySlots[i];

                if (playerCard.IsEmpty || !myCard.IsEmpty) continue;

                var playerAttack = playerCard.Card.CardData.Attack.Average();
                        
                if (playerAttack > StateOwner.Settings.ImmediateRespondToDangerLevelDefensive)
                    immediateRespondSlots.Add(i);
                else otherDangerSlots.Add(i);
            }

            var hopeUsed = 0;
            foreach (var index in immediateRespondSlots)
            {
                CardData? card;
                
                if (StateOwner.Settings.TryToMatchDangerLevelsDefensive)
                {
                    // If we can't play a card -> fall back to the least costly one
                    card = GetCardWithMatchingCost(index) ?? GetCardWithLeastCost();
                }
                else
                {
                    card = GetCardWithLeastCost();
                }

                if (card == null) // If we can't use a card with the least cost => we can't use any.
                {
                    hopeUsed = int.MaxValue;
                    break;
                }

                StateOwner.PlayCard(card.Value, index);
                hopeUsed += card.Value.Cost;
            }

            foreach (var index in otherDangerSlots)
            {
                if (hopeUsed > StateOwner.Settings.MaxHopeUsagePerTurnDefensive) break;
                
                CardData? card;
                
                if (StateOwner.Settings.TryToMatchDangerLevelsDefensive)
                {
                    // If we can't play a card -> fall back to the least costly one
                    card = GetCardWithMatchingCost(index) ?? GetCardWithLeastCost();
                }
                else
                {
                    card = GetCardWithLeastCost();
                }

                // If we can't use a card with the least cost => we can't use any.
                if (card == null) break;

                StateOwner.PlayCard(card.Value, index);
                hopeUsed += card.Value.Cost;
            }
        }

        private CardData? GetCardWithLeastCost()
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

        private CardData? GetCardWithMatchingCost(float match)
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
        
        public override void EnterState() { }
        public override void ExitState() { }
        public override void FrameUpdate() { }
        public override void FixedFrameUpdate() { }
    }
}