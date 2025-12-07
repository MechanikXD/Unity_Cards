using UnityEngine;

namespace UI.View.GameView
{
    public class StatView : CanvasView
    {
        [SerializeField] private PlayerStatView _playerView;
        [SerializeField] private PlayerStatView _otherView;
    }
}