using UnityEngine;
using UnityEngine.UI;

namespace UI.View.MainMenuView
{
    public class DeckView : CanvasView
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Transform _playerCardsTransform;
        [SerializeField] private Transform _otherCardsTransform;

        public void MoveCardToPlayer(Transform card)
        {
            card.SetParent(_playerCardsTransform);
        }

        public void MoveCardToOther(Transform card)
        {
            card.SetParent(_otherCardsTransform);
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(UIManager.Instance.ExitLastCanvas);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(UIManager.Instance.ExitLastCanvas);
        }
    }
}