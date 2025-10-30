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
            _changeDeckButton.onClick.AddListener(UIManager.Instance.EnterUICanvas<SettingsView>);
            _settingsButton.onClick.AddListener(UIManager.Instance.EnterUICanvas<DeckView>);
            _exitButton.onClick.AddListener(ApplicationManager.ExitApplication);
        }
        
        private void OnDisable()
        {
            _newGameButton.onClick.RemoveListener(ApplicationManager.EnterGameScene);
            _changeDeckButton.onClick.RemoveListener(UIManager.Instance.EnterUICanvas<SettingsView>);
            _settingsButton.onClick.RemoveListener(UIManager.Instance.EnterUICanvas<DeckView>);
            _exitButton.onClick.RemoveListener(ApplicationManager.ExitApplication);
        }
    }
}