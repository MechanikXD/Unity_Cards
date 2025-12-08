using Cards.Board;
using Dialogs;
using UI;
using UI.View.GameView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Structure.Managers
{
    public class GameManager : SingletonBase<GameManager>
    {
        [SerializeField] private BoardModel _board;
        [SerializeField] private DialogDataBase _dialogDataBase;
        
        public bool ActIsFinished { get; private set; }
        public BoardModel Board => _board;

        private void Start()
        {
            var gs = SessionManager.Instance;
            Board.StartGame(gs.DifficultySettings, !gs.HadLoadedData);
            gs.HadLoadedData = false;
        }
        
        protected override void Initialize() { }

        public void WinAct()
        {
            ActIsFinished = true;
            Board.FinishAct();
            UIManager.Instance.GetHUDCanvas<ScreenFade>()
                .FadeIn(() => SceneManager.LoadScene("Dialogs"));

            void InitializeDialog(Scene scene, LoadSceneMode mode)
            {
                if (scene.name != "Dialogs") return;

                var first = _dialogDataBase.GetRandom();
                var second = _dialogDataBase.GetRandom();
                DialogSceneController.Instance.Load(new[]
                {
                    new DialogSettings(first._backgroundImagePath, first._foregroundImagePath,
                        first._dialogs, SessionManager.Instance.GetRandomPlayerBuffOptions(3)),
                    new DialogSettings(second._backgroundImagePath, second._foregroundImagePath,
                        second._dialogs, SessionManager.Instance.GetRandomEnemyBuffOptions(3))
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
            UIManager.Instance.GetUICanvas<GameResultView>().SetStats(":)");
            UIManager.Instance.EnterUICanvas<GameResultView>();
        }
    }
}