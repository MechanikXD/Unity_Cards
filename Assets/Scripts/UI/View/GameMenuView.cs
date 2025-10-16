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
            _resumeButton.onClick.AddListener(UIManager.Instance.ExitLastCanvas);
            _restartButton.onClick.AddListener(RestartScene);
            _exitButton.onClick.AddListener(ExitApplication);
        }

        private void UnSubscribeFromEvents()
        {
            _resumeButton.onClick.RemoveListener(UIManager.Instance.ExitLastCanvas);
            _restartButton.onClick.RemoveListener(RestartScene);
            _exitButton.onClick.RemoveListener(ExitApplication);
        }
        
        private void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        private void ExitApplication() => Application.Quit();
    }
}