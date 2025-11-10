using System.Linq;
using Core.Cards.Card.Data;
using Core.Cards.Deck;
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

        [SerializeField] private int[] _defaultDeck;
        [SerializeField] private DeckCardModel _prefab;
        [SerializeField] private CardDataBank _db;

        public PlayerCardDragArea PlayerCards => _playerCardsArea;
        public OtherCardDragArea OtherCards => _otherCardsArea;

        protected override void Awake()
        {
            base.Awake();
            for (var i = 0; i < _db.Count; i++)
            {
                var newModel = Instantiate(_prefab);
                newModel.Set(_db.Get(i));

                if (_defaultDeck.Contains(i)) _playerCardsArea.AddCard(newModel);
                else _otherCardsArea.AddCard(newModel);
            }
        }

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