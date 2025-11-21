using System.Collections.Generic;
using Core.Behaviour.StateMachine;
using Core.Cards.Card.Data;
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
            var otherDangerSlots = new List<int>();
            var immediateRespondSlots = new List<int>();
            // Danger level will appear only if player card can directly attack enemy
            for (var i = 0; i < Board.EnemySlots.Length; i++)
            {
                var playerCard = Board.PlayerSlots[i];
                var myCard = Board.EnemySlots[i];

                if (playerCard.IsEmpty || !myCard.IsEmpty) continue;

                var playerAttack = playerCard.Card.Data.Attack.Average();
                        
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
                    var playerCard = Board.PlayerSlots[index];
                    if (playerCard.IsEmpty)
                    {
                        card = GetCardWithLeastCost();
                    }
                    else
                    {
                        var attack = playerCard.Card.Data.Attack.Average();
                        card = GetCardWithMatchingCost(attack) ?? GetCardWithLeastCost();
                    }
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
            
            StateOwner.FinishTurn();
        }

        public override void EnterState() { }
        public override void ExitState() { }
    }
}