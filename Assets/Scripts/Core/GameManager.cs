using Core.Behaviour;
using Core.Cards.Board;
using Enemy;
using Storage;
using UI;
using UI.View;
using UI.View.MainMenuView;
using UnityEngine;

namespace Core
{
    public class GameManager : SingletonBase<GameManager>
    {
        [SerializeField] private BoardModel _board;
        [SerializeField] private EnemyDifficultySettings _difficultySettings;
        
        [SerializeField] private int[] _otherCardsIds;
        public bool GameIsFinished { get; private set; }
        
        public BoardModel Board => _board;
        public EnemyDifficultySettings DifficultySettings => _difficultySettings;
        
        protected override void Initialize()
        {
            var strings = StorageProxy.Get<string>(DeckView.DeckIDStorageKey).Split(',');
            var ids = new int[strings.Length];
            for (var i = 0; i < strings.Length; i++) ids[i] = int.Parse(strings[i]);
            
            _board.StartGame(ids, _otherCardsIds);
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