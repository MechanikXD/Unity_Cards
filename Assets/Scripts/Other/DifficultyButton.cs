using Core;
using Core.SessionStorage;
using Enemy;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Other
{
    public class DifficultyButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _title;
        private EnemyDifficultySettings _settings;

        private void OnEnable()
        {
            _button.onClick.AddListener(LoadGameSceneWithSettings);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(LoadGameSceneWithSettings);
        }

        public void SetData(EnemyDifficultySettings settings)
        {
            _title.SetText(settings.DifficultyName);
            _settings = settings;
        }

        private void LoadGameSceneWithSettings()
        {
            SceneManager.LoadScene("GameScene");

            void PassSettings(Scene scene, LoadSceneMode loadSceneMode)
            {
                GameStorage.Instance.SetSettings(_settings);
                SceneManager.sceneLoaded -= PassSettings;
            }
            
            SceneManager.sceneLoaded += PassSettings;
        }
    }
}