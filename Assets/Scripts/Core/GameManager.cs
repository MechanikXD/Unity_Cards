using Core.Behaviour;
using Core.Cards.Board;
using Core.Cards.Card.Data;
using UnityEngine;

namespace Core
{
    public class GameManager : SingletonBase<GameManager>
    {
        [SerializeField] private BoardModel _board;
        
        // TODO: THIS IS TEMP, replace with actual values
        [SerializeField] private CardData[] _playerCards;
        [SerializeField] private CardData[] _otherCards;
        
        public BoardModel Board => _board;
        
        protected override void Initialize()
        {
            _board.StartGame(_playerCards, _otherCards);
        }

        public void WinGame()
        {
            
        }
        
        public void GameLoose()
        {
            
        }
    }
}