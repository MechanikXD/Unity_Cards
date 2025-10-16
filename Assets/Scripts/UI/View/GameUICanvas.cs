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
            _pauseButton.onClick.AddListener(UIManager.Instance.EnterUICanvas<GameMenuView>);
        }

        private void UnSubscribeFromEvents()
        {
            _playButton.onClick.RemoveListener(PlayTurn);
            _pauseButton.onClick.RemoveListener(UIManager.Instance.EnterUICanvas<GameMenuView>);
        }

        private void PlayTurn()
        {
            GameManager.Instance.Board.PlayTurnAsync().Forget();
        }
    }
}