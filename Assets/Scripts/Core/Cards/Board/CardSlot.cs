using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using Other;
using UnityEngine;

namespace Core.Cards.Board
{
    public class CardSlot : MonoBehaviour
    {
        private const int SORTING_ORDER = 3;
        [SerializeField] private int _cardIndex;
        [SerializeField] private float _cardMoveSpeed = 1f;
        [SerializeField] private bool _canSnapTo = true;
        [SerializeField] private Vector3 _cardPosition =  new Vector3(0, 0, 0);
        [SerializeField] private BoardModel _board;
        
        public bool IsEmpty { get; private set; } = true;
        public CardModel Card { get; private set; }

        public void Attach(CardModel card)
        {
            IsEmpty = false;
            Card = card;
            card.transform.SetParent(transform);
            card.transform.localScale = Vector3.one;
            card.SortingGroup.sortingOrder = SORTING_ORDER;
            card.transform.MoveToLocalAsync(_cardPosition, _cardMoveSpeed, 
                this.destroyCancellationToken).Forget();
        }

        public CardModel Detach()
        {
            var cardModel = Card;
            Card.transform.parent = null;
            Card = null;
            IsEmpty = true;
            return cardModel;
        }
    }
}