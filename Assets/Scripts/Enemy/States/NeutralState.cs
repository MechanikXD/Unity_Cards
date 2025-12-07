using System.Collections.Generic;
using Other.Extensions;
using Structure.StateMachine;

namespace Enemy.States
{
    public class NeutralState : EnemyState
    {
        public NeutralState(StateMachine<EnemyBehaviour> sm, EnemyBehaviour owner) : base(sm, owner) { }

        protected override float GetCurrentStateDangerMultiplier() => 1f;

        public override void PlayTurn()
        {
            var rng = new System.Random();
            var cardsShuffle = Hand.CardsInHand.Shuffled();
            var shuffledIndex = 0;
            
            var cardsToPlay = rng.Next(1, StateOwner.Settings.MaxCardCountPerTurn + 1);
            var dangerSlots = new List<int>();
            var emptySlots = new List<int>();
            for (var i = 0; i < Board.EnemySlots.Length; i++)
            {
                var playerCard = Board.PlayerSlots[i];
                var myCard = Board.EnemySlots[i];

                if (!playerCard.IsEmpty && myCard.IsEmpty)
                {
                    var danger = playerCard.Card.Data.Attack.Average();
                    if (danger >= StateOwner.Settings.MinDangerLevelToRespondNeutral)
                    {
                        dangerSlots.Add(i);
                    }
                    else
                    {
                        emptySlots.Add(i);
                    }
                }
                else if (myCard.IsEmpty)
                {
                    emptySlots.Add(i);
                }
            }
            emptySlots.Shuffle();

            foreach (var index in dangerSlots)
            {
                if (cardsToPlay <= 0) break;
                
                if (Hand.CanUseCard(cardsShuffle[shuffledIndex].Cost))
                {
                    StateOwner.PlayCard(Hand.GetCardFromHand(cardsShuffle[shuffledIndex]), index);
                    cardsToPlay--;
                }
                
                shuffledIndex++;
                if (shuffledIndex >= cardsShuffle.Count) break;
            }

            if (shuffledIndex >= cardsShuffle.Count)
            {
                StateOwner.FinishTurn();
                return;
            }
            
            foreach (var index in emptySlots)
            {
                if (cardsToPlay <= 0) break;
                
                if (Hand.CanUseCard(cardsShuffle[shuffledIndex].Cost))
                {
                    StateOwner.PlayCard(Hand.GetCardFromHand(cardsShuffle[shuffledIndex]), index);
                    cardsToPlay--;
                }
                
                shuffledIndex++;
                if (shuffledIndex >= cardsShuffle.Count) break;
            }
            
            StateOwner.FinishTurn();
        }

        public override void EnterState() { }
        public override void ExitState() { }
    }
}