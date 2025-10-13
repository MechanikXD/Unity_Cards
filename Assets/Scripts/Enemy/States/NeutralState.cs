using System.Collections.Generic;
using Core.Behaviour.StateMachine;
using Other;

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
                    var danger = playerCard.Card.CardData.Attack.Average();
                    if (danger >= StateOwner.Settings.MinDangerLevelToRespondNeutral)
                    {
                        dangerSlots.Add(i);
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
                if (shuffledIndex >= cardsShuffle.Count) return;
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
                if (shuffledIndex >= cardsShuffle.Count) return;
            }
        }
        
        public override void EnterState() { }
        public override void ExitState() { }
        public override void FrameUpdate() { }
        public override void FixedFrameUpdate() { }
    }
}