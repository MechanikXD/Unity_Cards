using Core.SessionStorage;
using Enemy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.View.MainMenuView
{
    public class MainView : CanvasView
    {
        [SerializeField] private EnemyDifficultySettings[] _difficultySettings;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _changeDeckButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        private void OnEnable()
        {
            _newGameButton.onClick.AddListener(EnterNewGameScene);
            _continueButton.onClick.AddListener(ContinueGame);
            _changeDeckButton.onClick.AddListener(OpenDeck);
            _settingsButton.onClick.AddListener(OpenSettings);
            _exitButton.onClick.AddListener(ExitApplication);
        }

        protected override void Awake()
        {
            base.Awake();
            _continueButton.interactable = GameSerializer.HasSavedData();
        }

        private void OnDisable()
        {
            _newGameButton.onClick.RemoveListener(EnterNewGameScene);
            _continueButton.onClick.RemoveListener(ContinueGame);
            _changeDeckButton.onClick.RemoveListener(OpenDeck);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _exitButton.onClick.RemoveListener(ExitApplication);
        }

        private static void OpenSettings() => UIManager.Instance.EnterUICanvas<SettingsView>();
        private static void OpenDeck() => UIManager.Instance.EnterUICanvas<DeckView>();

        private void EnterNewGameScene()
        {
            GameSerializer.Clear();
            UIManager.Instance.GetHUDCanvas<DifficultySelectionView>().CreateButtons(_difficultySettings);
            UIManager.Instance.EnterHUDCanvas<DifficultySelectionView>();
        }

        private void ContinueGame()
        {
            var data = GameSerializer.Deserialize();
            SceneManager.LoadScene(data.scene);

            void DeserializeOnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
            {
                if (scene.name == "MainMenu") return;
                GameStorage.Instance.Deserialize(data.storage);
                SceneManager.sceneLoaded -= DeserializeOnSceneLoad;
            }

            SceneManager.sceneLoaded += DeserializeOnSceneLoad;
        }

        private static void ExitApplication() => Application.Quit();
    }
}