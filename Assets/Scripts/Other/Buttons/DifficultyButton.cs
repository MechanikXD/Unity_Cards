using Enemy;
using Structure.Managers;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Other.Buttons
{
    public class DifficultyButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _title;
        private EnemyDifficultySettings _settings;

        private void OnEnable() => _button.onClick.AddListener(LoadGameSceneWithSettings);

        private void OnDisable() => _button.onClick.RemoveListener(LoadGameSceneWithSettings);

        public void SetData(EnemyDifficultySettings settings)
        {
            _title.SetText(settings.DifficultyName);
            _settings = settings;
        }

        private void LoadGameSceneWithSettings()
        {
            UIManager.Instance.GetHUDCanvas<ScreenFade>().FadeIn(
                () => SceneManager.LoadScene("GameScene"));

            void PassSettings(Scene scene, LoadSceneMode loadSceneMode)
            {
                SessionManager.Instance.SetSettings(_settings);
                SceneManager.sceneLoaded -= PassSettings;
            }
            
            SceneManager.sceneLoaded += PassSettings;
        }
    }
}