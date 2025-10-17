using System.Collections.Generic;
using Core.Behaviour.StateMachine;
using Core.Cards.Card.Data;
using Other;

namespace Enemy.States
{
    public class AggressiveState : EnemyState
    {
        public AggressiveState(StateMachine<EnemyBehaviour> sm, EnemyBehaviour owner) : base(sm, owner) { }

        protected override float GetCurrentStateDangerMultiplier() =>
            StateOwner.Settings.DangerMultiplyFactorAggressive;

        public override void PlayTurn()
        {
            var counteractSlots = new List<int>();
            var emptySlots = new List<int>();
            var totalPressure = 0f;
            // Only when card slot can attack player directly (player has no card) it is 0
            // Otherwise there will be some kind of value.
            for (var i = 0; i < Board.EnemySlots.Length; i++)
            {
                var playerCard = Board.PlayerSlots[i];
                var myCard = Board.EnemySlots[i];
                
                switch (playerCard.IsEmpty)
                {
                    // both empty
                    case true when myCard.IsEmpty:
                        emptySlots.Add(i);
                        break;
                    // player card only
                    case false when myCard.IsEmpty:
                        var danger = playerCard.Card.CardData.Attack.Average();
                        if (danger >= StateOwner.Settings.IgnoreDangerLevelAggressive)
                        {
                            counteractSlots.Add(i);
                        }
                        else
                        {
                            emptySlots.Add(i);
                        }
                        break;
                    // My card only
                    case true when !myCard.IsEmpty:
                        var attackValue = myCard.Card.CardData.Attack.Average();
                        totalPressure += attackValue;
                        break;
                }
            }

            foreach (var index in counteractSlots)
            {
                CardData? card;
                
                if (!StateOwner.Settings.TryToPreserveHope)
                {
                    card = GetCardWithLeastCost();
                }
                else
                {
                    var attack = Board.PlayerSlots[index].Card.CardData.Attack.Average();
                    card = GetCardWithMatchingCost(attack) ?? GetCardWithLeastCost();
                }

                // If we can't use a card with the least cost => we can't use any.
                if (card == null) break;
                
                StateOwner.PlayCard(card.Value, index);
            }

            foreach (var index in emptySlots)
            {
                if (totalPressure >= StateOwner.Settings.MaxPlayerPressureAggressive) break;
                
                CardData? card;
                
                if (!StateOwner.Settings.TryToPreserveHope)
                {
                    card = GetCardWithLeastCost();
                }
                else
                {
                    var playerCard = Board.PlayerSlots[index];
                    if (playerCard.IsEmpty)
                    {
                        card = GetCardWithLeastCost();
                    }
                    else
                    {
                        var attack = playerCard.Card.CardData.Attack.Average();
                        card = GetCardWithMatchingCost(attack) ?? GetCardWithLeastCost();
                    }
                }

                // If we can't use a card with the least cost => we can't use any.
                if (card == null) break;
                
                StateOwner.PlayCard(card.Value, index);
            }
            
            StateOwner.FinishTurn();
        }

        public override void EnterState() { }
        public override void ExitState() { }
    }
}