using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.View
{
    public class GameResultView : CanvasView
    {
        [SerializeField] private TMP_Text _title;
        
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _exitButton;
        
        private void OnEnable() => SubscribeToEvents();

        private void OnDisable() => UnSubscribeFromEvents();
        
        public void SetTitle(string title) => _title.SetText(title);

        private void SubscribeToEvents()
        {
            _restartButton.onClick.AddListener(RestartScene);
            _exitButton.onClick.AddListener(ExitApplication);
        }

        private void UnSubscribeFromEvents()
        {
            _restartButton.onClick.RemoveListener(RestartScene);
            _exitButton.onClick.RemoveListener(ExitApplication);
        }
        
        private void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        private void ExitApplication() => Application.Quit();
    }
}