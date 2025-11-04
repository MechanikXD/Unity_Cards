using Core.Behaviour;
using Core.Cards.Board;
using Enemy;
using UI;
using UI.Settings;
using UI.Settings.Types;
using UI.View;
using UI.View.MainMenuView;
using UnityEngine;

namespace Core
{
    public class GameManager : SingletonBase<GameManager>
    {
        [SerializeField] private BoardModel _board;
        [SerializeField] private EnemyDifficultySettings _difficultySettings;
        
        // TODO: THIS IS TEMP, replace with actual values
        [SerializeField] private int[] _playerCardsIds;
        [SerializeField] private int[] _otherCardsIds;
        public bool GameIsFinished { get; private set; }
        
        public BoardModel Board => _board;
        public EnemyDifficultySettings DifficultySettings => _difficultySettings;
        
        protected override void Initialize()
        {
            _board.StartGame(_playerCardsIds, _otherCardsIds);
        }

        public void WinGame()
        {
            GameIsFinished = true;
            UIManager.Instance.GetUICanvas<GameResultView>().SetTitle("Victory!");
            UIManager.Instance.EnterUICanvas<GameResultView>();
        }
        
        public void GameLoose()
        {
            GameIsFinished = true;
            UIManager.Instance.GetUICanvas<GameResultView>().SetTitle("Defeat");
            UIManager.Instance.EnterUICanvas<GameResultView>();
        }
    }
}