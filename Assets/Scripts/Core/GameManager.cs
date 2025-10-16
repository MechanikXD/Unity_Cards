using Core.Behaviour;
using Core.Cards.Board;
using Enemy;
using UnityEngine;

namespace Core
{
    public class GameManager : SingletonBase<GameManager>
    {
        [SerializeField] private BoardModel _board;
        [SerializeField] private Transform _controlCanvas;
        [SerializeField] private EnemyDifficultySettings _difficultySettings;
        
        // TODO: THIS IS TEMP, replace with actual values
        [SerializeField] private int[] _playerCardsIds;
        [SerializeField] private int[] _otherCardsIds;
        public bool GameIsFinished { get; private set; }
        
        public BoardModel Board => _board;
        public Transform ControlCanvas => _controlCanvas;
        public EnemyDifficultySettings DifficultySettings => _difficultySettings;
        
        protected override void Initialize()
        {
            _board.StartGame(_playerCardsIds, _otherCardsIds);
        }

        public void WinGame()
        {
            GameIsFinished = true;
            Debug.Log("Game Won");
        }
        
        public void GameLoose()
        {
            GameIsFinished = true;
            Debug.Log("Game Lost");
        }
    }
}