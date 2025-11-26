using Core.Behaviour;
using Core.Cards.Board;
using Enemy;
using Player.Progression.Buffs;
using Storage;
using UI;
using UI.View.GameView;
using UI.View.MainMenuView;
using UnityEngine;

namespace Core
{
    public class GameManager : SingletonBase<GameManager>
    {
        [SerializeField] private BoardModel _board;
        [SerializeField] private BuffDataBase _buffDb;
        [SerializeField] private EnemyDifficultySettings _difficultySettings;
        
        public bool ActIsFinished { get; private set; }

        public EnemyDifficultySettings DifficultySettings => _difficultySettings;
        public int CurrentTier { get; private set; }
        public BoardModel Board => _board;
        public BuffDataBase BuffDb => _buffDb;

        protected override void Initialize() { }

        private void Start()
        {
            var strings = StorageProxy.Get<string>(DeckView.DeckIDStorageKey).Split(',');
            var ids = new int[strings.Length];
            for (var i = 0; i < strings.Length; i++) ids[i] = int.Parse(strings[i]);
            
            _board.StartGame(ids, _difficultySettings);
        }

        public void WinAct()
        {
            ActIsFinished = true;
            Board.FinishAct();
            UIManager.Instance.EnterUICanvas<BuffSelectionView>();
            UIManager.Instance.GetUICanvas<BuffSelectionView>().LoadRandomPlayerBuffs(CurrentTier);
        }
        
        public void LooseAct()
        {
            ActIsFinished = true;
            FinalizeGame();
            UIManager.Instance.GetUICanvas<GameResultView>().SetTitle(":)");
            UIManager.Instance.EnterUICanvas<GameResultView>();
        }

        private void FinalizeGame()
        {
            
        }
    }
}