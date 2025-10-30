using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.View
{
    public class GameMenuView : CanvasView
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _exitButton;

        private void OnEnable() => SubscribeToEvents();
        private void OnDisable() => UnSubscribeFromEvents();

        private void SubscribeToEvents()
        {
            _resumeButton.onClick.AddListener(ExitCanvas);
            _restartButton.onClick.AddListener(RestartScene);
            _exitButton.onClick.AddListener(ExitApplication);
        }

        private void UnSubscribeFromEvents()
        {
            _resumeButton.onClick.RemoveListener(ExitCanvas);
            _restartButton.onClick.RemoveListener(RestartScene);
            _exitButton.onClick.RemoveListener(ExitApplication);
        }

        private void ExitCanvas()
        {
            if (UIManager.Instance != null) UIManager.Instance.ExitLastCanvas();
        }

        private void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        private void ExitApplication() => Application.Quit();
    }
}