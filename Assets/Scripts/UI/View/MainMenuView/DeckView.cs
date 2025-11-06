using Player.Deck;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View.MainMenuView
{
    public class DeckView : CanvasView
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private PlayerCardDragArea _playerCardsArea;
        [SerializeField] private OtherCardDragArea _otherCardsArea;

        public PlayerCardDragArea PlayerCards => _playerCardsArea;
        public OtherCardDragArea OtherCards => _otherCardsArea;

        private void OnEnable()
        {
            _backButton.onClick.AddListener(ExitLastCanvas);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(ExitLastCanvas);
        }

        private void ExitLastCanvas()
        {
            if (UIManager.Instance != null) UIManager.Instance.ExitLastCanvas();
        }
    }
}