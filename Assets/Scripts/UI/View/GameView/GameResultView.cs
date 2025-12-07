using SaveLoad;
using Structure.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.View.GameView
{
    public class GameResultView : CanvasView
    {
        [SerializeField] private TMP_Text _statText;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _mainMenuButton;
        
        private void OnEnable() 
        {
            _exitButton.onClick.AddListener(ExitApplication);
            _mainMenuButton.onClick.AddListener(ToMainMenu);
            GameSerializer.Clear();
        }

        private void OnDisable()
        {
            _exitButton.onClick.RemoveListener(ExitApplication);
            _mainMenuButton.onClick.RemoveListener(ToMainMenu);
        }
        
        public void SetStats(string text) => _statText.SetText(text);
        
        private static void ToMainMenu()
        {
            GameSerializer.Clear();
            Destroy(GameStorage.Instance.gameObject);
            SceneManager.LoadScene("MainMenu");
        }

        private static void ExitApplication() => Application.Quit();
    }
}