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
        [SerializeField] private AudioClip[] _music;
        [SerializeField] private int _musicChangeStep = 5;
        
        [SerializeField] private BoardModel _board;
        [SerializeField] private DialogDataBase _dialogDataBase;
        
        public bool ActIsFinished { get; private set; }
        public BoardModel Board => _board;

        private void Start()
        {
            var gs = SessionManager.Instance;
            Board.StartGame(gs.DifficultySettings, !gs.HadLoadedData);
            UIManager.Instance.GetHUDCanvas<GameHUDView>().LoadBuffList(gs.PlayerBuffs, gs.EnemyBuffs);
            gs.HadLoadedData = false;
            PlayMusic();
        }
        
        protected override void Initialize() { }

        private void PlayMusic()
        {
            var act = SessionManager.Instance.CurrentAct;

            if (act >= (_music.Length - 1) * _musicChangeStep)
            {
                AudioManager.Instance.PlayMusic(_music[^1]);
                return;
            }
            
            for (var i = 0; i < _music.Length; i++)
            {
                if (act < (i + 1) * _musicChangeStep)
                {
                    AudioManager.Instance.PlayMusic(_music[i]);
                    break;
                }
            }
        }
        
        public void WinAct()
        {
            if (ActIsFinished) return;
            ActIsFinished = true;
            UIManager.Instance.GetHUDCanvas<ScreenFade>()
                .FadeIn(() => SceneManager.LoadScene("Dialogs"));

            void InitializeDialog(Scene scene, LoadSceneMode mode)
            {
                SessionManager.Instance.PlayerData.Reset();
                if (scene.name != "Dialogs") return;

                var first = _dialogDataBase.GetRandom(DialogType.PlayerBuff);
                var second = _dialogDataBase.GetRandom(DialogType.EnemyBuff);
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
            if (ActIsFinished) return;
            ActIsFinished = true;
            FinalizeGame();
        }

        private void FinalizeGame()
        {
            UIManager.Instance.GetUICanvas<GameResultView>()
                .SetStats(SessionManager.Instance.GetFormatedStats());
            UIManager.Instance.EnterUICanvas<GameResultView>();
        }
    }
}