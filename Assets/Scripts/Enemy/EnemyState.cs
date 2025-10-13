using System.Linq;
using Core.Behaviour.StateMachine;
using Core.Cards.Board;
using Core.Cards.Hand;
using Enemy.States;
using Other;

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
    }
}