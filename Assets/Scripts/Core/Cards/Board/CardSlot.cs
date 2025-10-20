using System.Threading;
using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Cards.Board
{
    public class CardSlot : MonoBehaviour
    {
        private const float CARD_MOVE_SPEED = 3f;
        private const float CARD_SNAP_DISTANCE = 10f;
        private const int SORTING_ORDER = 3;
        private readonly CancellationTokenSource _cts = new  CancellationTokenSource();
        private CardModel _child;

        [SerializeField] private bool _canSnapTo = true;
        [SerializeField] private BoardModel _board;
        [SerializeField] private int _cardIndex;
        [SerializeField] private Vector3 _cardPosition =  new Vector3(0, 0, 0);

        public bool IsEmpty { get; private set; } = true;
        public CardModel Card => _child;
        
        public void Attach(CardModel card)
        {
            IsEmpty = false;
            _child = card;
            card.transform.SetParent(transform);
            card.transform.localScale = Vector3.one;
            card.SortingGroup.sortingOrder = SORTING_ORDER;
            MoveCardToPositionAsync(card.transform, _cts.Token).Forget();
        }

        public CardModel Detach()
        {
            var cardModel = _child;
            _child.transform.parent = null;
            _child = null;
            IsEmpty = true;
            return cardModel;
        }

        private async UniTask MoveCardToPositionAsync(Transform cardTransform, CancellationToken ct = default)
        {
            var moveDirection = _cardPosition - cardTransform.localPosition;
            while (Vector3.Distance(cardTransform.localPosition, _cardPosition) > CARD_SNAP_DISTANCE)
            {
                cardTransform.Translate(moveDirection * (Time.deltaTime * CARD_MOVE_SPEED));
                await UniTask.NextFrame(cancellationToken:ct);
            }

            cardTransform.localPosition = _cardPosition; // Snap to final location
        }

        private void OnDestroy() => _cts.Cancel();
    }
}