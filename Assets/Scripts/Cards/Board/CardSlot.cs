using Cards.Card;
using Cysharp.Threading.Tasks;
using Other;
using Other.Interactions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cards.Board
{
    public class CardSlot : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        private const int SORTING_ORDER = 3;
        [SerializeField] private int _cardIndex;
        [SerializeField] private float _cardMoveSpeed = 10f;
        [SerializeField] private bool _canSnapTo = true;
        [SerializeField] private Vector3 _cardPosition =  new Vector3(0, 0, 0);
        [SerializeField] private BoardModel _board;
        
        public bool CanAttach => _canSnapTo;
        public bool IsEmpty { get; private set; } = true;
        public CardModel Card { get; private set; }

        public void Attach(CardModel card, bool instantMove=false)
        {
            IsEmpty = false;
            Card = card;
            card.transform.SetParent(transform);
            card.transform.localScale = Vector3.one;
            card.SortingGroup.sortingOrder = SORTING_ORDER;
            if (instantMove) card.transform.position = _cardPosition;
            else card.MoveToLocalAsync(_cardPosition, _cardMoveSpeed).Forget();
        }

        public CardModel Detach()
        {
            var cardModel = Card;
            Card.transform.parent = null;
            Card = null;
            IsEmpty = true;
            return cardModel;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsEmpty || !AnyRequireMove())
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
                    card.MoveCard(this);
                    break;
                }
            }
        }

        public bool AnyRequireMove()
        {
            var playerSlots = _board.PlayerSlots;
            foreach (var slot in playerSlots)
            {
                if (!slot.IsEmpty && slot.Card.RequestMove)
                {
                    return true;
                }
            }

            return false;
        }

        // Required to implement OnPointerUp
        public void OnPointerDown(PointerEventData eventData) { }
    }
}