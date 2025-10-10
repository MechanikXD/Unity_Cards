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
        }

        private void UnSubscribeFromEvents()
        {
            _playButton.onClick.RemoveListener(PlayTurn);
        }

        private void PlayTurn()
        {
            GameManager.Instance.Board.PlayTurnAsync().Forget();
        }
    }
}