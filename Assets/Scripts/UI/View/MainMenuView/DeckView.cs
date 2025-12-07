using System.Linq;
using Cards.Card.Data;
using Cards.Deck;
using Other.Interactions;
using Structure;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View.MainMenuView
{
    public class DeckView : CanvasView
    {
        public const string DeckIDStorageKey = "Deck";

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
            int[] ids;
            if (StorageProxy.HasKey(DeckIDStorageKey))
            {
                var strings = StorageProxy.Get<string>(DeckIDStorageKey).Split(',');
                ids = new int[strings.Length];
                for (var i = 0; i < strings.Length; i++)
                {
                    ids[i] = int.Parse(strings[i]);
                }
            }
            else ids = _defaultDeck;
            
            for (var i = 0; i < _db.Count; i++)
            {
                var newModel = Instantiate(_prefab);
                newModel.Set(_db.Get(i));

                if (ids.Contains(i)) _playerCardsArea.AddCard(newModel);
                else _otherCardsArea.AddCard(newModel);
            }
        }

        private void SaveCards()
        {
            var ids = _playerCardsArea.CardCount <= 0 ? _defaultDeck : _playerCardsArea.GetCardIDs;
            StorageProxy.Set(DeckIDStorageKey, string.Join(',', ids));
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
            SaveCards();
            HideInfoOnClick.HideInfo();
            if (UIManager.Instance != null) UIManager.Instance.ExitLastCanvas();
        }
    }
}