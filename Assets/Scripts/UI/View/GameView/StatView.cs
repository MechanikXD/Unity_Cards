using UnityEngine;

namespace UI.View
{
    public class StatView : CanvasView
    {
        [SerializeField] private PlayerStatView _playerView;
        [SerializeField] private PlayerStatView _otherView;
    }
}