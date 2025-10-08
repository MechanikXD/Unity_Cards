using Core.Behaviour;
using Core.Cards.Board;
using UnityEngine;

namespace Core
{
    public class GameManager : SingletonBase<GameManager>
    {
        [SerializeField] private BoardModel _board;
        [SerializeField] private Transform _controlCanvas;
        
        // TODO: THIS IS TEMP, replace with actual values
        [SerializeField] private int[] _playerCardsIds;
        [SerializeField] private int[] _otherCardsIds;
        
        public BoardModel Board => _board;
        public Transform ControlCanvas => _controlCanvas;
        
        protected override void Initialize()
        {
            _board.StartGame(_playerCardsIds, _otherCardsIds);
        }

        public void WinGame()
        {
            Debug.Log("Game Won");
        }
        
        public void GameLoose()
        {
            Debug.Log("Game Lost");
        }
    }
}