using Cysharp.Threading.Tasks;
using Other.Interactions;
using Structure.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View.GameView
{
    public class GameHUDView : CanvasView
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _pauseButton;

        public void EnableButton(bool enable) => _playButton.interactable = enable;
        
        private void OnEnable() => SubscribeToEvents();

        private void OnDisable() => UnSubscribeFromEvents();

        private void SubscribeToEvents()
        {
            _playButton.onClick.AddListener(PlayTurn);
            _pauseButton.onClick.AddListener(Pause);
        }

        private void UnSubscribeFromEvents()
        {
            _playButton.onClick.RemoveListener(PlayTurn);
            _pauseButton.onClick.RemoveListener(Pause);
        }

        private void Pause()
        {
            if (UIManager.Instance != null) UIManager.Instance.EnterUICanvas<GameMenuView>();
        }

        private void PlayTurn()
        {
            HideInfoOnClick.HideAll();
            if (GameManager.Instance != null) GameManager.Instance.Board.PlayTurnAsync().Forget();
        }
    }
}