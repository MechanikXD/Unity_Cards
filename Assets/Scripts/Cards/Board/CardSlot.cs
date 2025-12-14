using Cards.Card;
using Cysharp.Threading.Tasks;
using Other.Interactions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cards.Board
{
    /// <summary> Slot for card located on the game board </summary>
    public class CardSlot : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        private const int SORTING_ORDER = 3;
        [SerializeField] private int _cardIndex; // Index of slot
        [SerializeField] private float _cardMoveSpeed = 10f;
        [SerializeField] private bool _canSnapTo = true; // Prevent placement on enemy slots
        [SerializeField] private Vector3 _cardPosition =  new Vector3(0, 0, 0);
        [SerializeField] private BoardModel _board;
        
        public bool CanSnapTo => _canSnapTo;
        public bool IsEmpty { get; private set; } = true;
        public CardModel Card { get; private set; }

        public void Deactivate() => _canSnapTo = false;
        public void Activate() => _canSnapTo = true;
        
        public void Attach(CardModel card, bool instantMove=false, bool reenableController=true)
        {
            IsEmpty = false;
            Card = card;
            card.transform.SetParent(transform);
            card.transform.localScale = Vector3.one;
            card.SortingGroup.sortingOrder = SORTING_ORDER;
            if (instantMove) card.transform.position = _cardPosition;
            else card.MoveToLocalAsync(_cardPosition, _cardMoveSpeed, reenableController:reenableController).Forget();
        }

        public CardModel Detach()
        {
            var cardModel = Card;
            Card.transform.parent = null;
            Card = null;
            IsEmpty = true;
            return cardModel;
        }

        // Show card actions if this slot has card attached
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_board.AnyRequireMove())
            {
                HideInfoOnClick.HideAll();
                return;
            }
            HideInfoOnClick.HideInfo();
            var playerSlots = _board.PlayerSlots;
            foreach (var slot in playerSlots)
            {
                if (slot._cardIndex != _cardIndex && !slot.IsEmpty && slot.Card.RequestMove)
                {
                    var card = slot.Detach();
                    if (IsEmpty) card.MoveCard(this);
                    else card.SwapCardsAsync(slot, this, _cardMoveSpeed * 2f).Forget();
                    break;
                }
            }
        }

        // Required to implement OnPointerUp
        public void OnPointerDown(PointerEventData eventData) { }
    }
}