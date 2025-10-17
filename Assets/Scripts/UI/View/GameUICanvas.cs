using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View
{
    public class GameUICanvas : CanvasView
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _pauseButton;

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
            if (GameManager.Instance != null) GameManager.Instance.Board.PlayTurnAsync().Forget();
        }
    }
}