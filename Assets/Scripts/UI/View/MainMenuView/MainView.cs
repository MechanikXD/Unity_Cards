using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View.MainMenuView
{
    public class MainView : CanvasView
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _changeDeckButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        private void OnEnable()
        {
            _newGameButton.onClick.AddListener(ApplicationManager.EnterGameScene);
            _changeDeckButton.onClick.AddListener(OpenDeck);
            _settingsButton.onClick.AddListener(OpenSettings);
            _exitButton.onClick.AddListener(ApplicationManager.ExitApplication);
        }
        
        private void OnDisable()
        {
            _newGameButton.onClick.RemoveListener(ApplicationManager.EnterGameScene);
            _changeDeckButton.onClick.RemoveListener(OpenDeck);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _exitButton.onClick.RemoveListener(ApplicationManager.ExitApplication);
        }
        
        private void OpenSettings() => UIManager.Instance.EnterUICanvas<SettingsView>();
        private void OpenDeck() => UIManager.Instance.EnterUICanvas<DeckView>();
    }
}