using Core.Behaviour;
using Core.Cards.Board;
using Core.Cards.Card;
using Core.SessionStorage;
using Enemy;
using Other.Dialog;
using UI;
using UI.View.GameView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class GameManager : SingletonBase<GameManager>
    {
        [SerializeField] private BoardModel _board;
        [SerializeField] private EnemyDifficultySettings _difficultySettings;
        
        public bool ActIsFinished { get; private set; }

        public EnemyDifficultySettings DifficultySettings => _difficultySettings;
        public BoardModel Board => _board;

        protected override void Initialize() { }

        private void Start()
        {
            _board.StartGame(_difficultySettings, !GameStorage.Instance.HadLoadedData);
            GameStorage.Instance.ResetLoadedDataBool();
        }

        public void WinAct()
        {
            ActIsFinished = true;
            Board.FinishAct();
            SceneManager.LoadScene("Dialogs");

            void InitializeDialog(Scene scene, LoadSceneMode mode)
            {
                if (scene != SceneManager.GetSceneByName("Dialogs")) return;
                
                DialogSceneController.Instance.Load(new[]
                {
                    new DialogSettings("Backgrounds/GreymoorBG",
                        new [] { "Hello", "World!" }, 
                        GameStorage.Instance.GetRandomPlayerBuffOptions(3)),
                    new DialogSettings("Backgrounds/GreymoorBG", 
                        new [] { "Hello", "Evil", "World!" },
                        GameStorage.Instance.GetRandomEnemyBuffOptions(3))
                });
                
                SceneManager.sceneLoaded -= InitializeDialog;
            }

            SceneManager.sceneLoaded += InitializeDialog;
        }
        
        public void LooseAct()
        {
            ActIsFinished = true;
            FinalizeGame();
        }

        private void FinalizeGame()
        {
            UIManager.Instance.GetUICanvas<GameResultView>().SetTitle(":)");
            UIManager.Instance.EnterUICanvas<GameResultView>();
        }
    }
}