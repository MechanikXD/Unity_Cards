using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.View.GameView
{
    public class GameMenuView : CanvasView
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _exitButton;

        private void OnEnable() => SubscribeToEvents();
        private void OnDisable() => UnSubscribeFromEvents();

        private void SubscribeToEvents()
        {
            _resumeButton.onClick.AddListener(ExitCanvas);
            _mainMenuButton.onClick.AddListener(ToMainMenu);
            _exitButton.onClick.AddListener(ExitApplication);
        }

        private void UnSubscribeFromEvents()
        {
            _resumeButton.onClick.RemoveListener(ExitCanvas);
            _mainMenuButton.onClick.RemoveListener(ToMainMenu);
            _exitButton.onClick.RemoveListener(ExitApplication);
        }

        private void ExitCanvas()
        {
            if (UIManager.Instance != null) UIManager.Instance.ExitLastCanvas();
        }

        private void ToMainMenu() => SceneManager.LoadScene("MainMenu");
        private void ExitApplication() => Application.Quit();
    }
}