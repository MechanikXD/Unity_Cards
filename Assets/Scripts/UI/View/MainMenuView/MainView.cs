using System;
using System.Collections.Generic;
using Core;
using Core.SessionStorage;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.View.MainMenuView
{
    public class MainView : CanvasView
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _changeDeckButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        
        private void OnEnable()
        {
            _newGameButton.onClick.AddListener(EnterNewGameScene);
            _changeDeckButton.onClick.AddListener(OpenDeck);
            _settingsButton.onClick.AddListener(OpenSettings);
            _exitButton.onClick.AddListener(ExitApplication);
        }
        
        private void OnDisable()
        {
            _newGameButton.onClick.RemoveListener(EnterNewGameScene);
            _changeDeckButton.onClick.RemoveListener(OpenDeck);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _exitButton.onClick.RemoveListener(ExitApplication);
        }
        
        private static void OpenSettings() => UIManager.Instance.EnterUICanvas<SettingsView>();
        private static void OpenDeck() => UIManager.Instance.EnterUICanvas<DeckView>();
        
        private static void EnterNewGameScene()
        {
            GameSerializer.Clear();
            SceneManager.LoadScene("GameScene");
        }

        private void ContinueGame()
        {
            var data = GameSerializer.Deserialize();
            SceneManager.LoadScene(data.scene);
            GameStorage.Instance.Deserialize(data.storage);
        }
        
        private static void ExitApplication() => Application.Quit();
    }
}