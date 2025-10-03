using System;
using System.Threading;
using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Cards.Board
{
    public class CardSlot : MonoBehaviour
    {
        private const float CART_MOVE_SPEED = 1f;
        private const float CART_SNAP_DISTANCE = 0.05f;
        private readonly CancellationTokenSource _cts = new  CancellationTokenSource();
        private CardModel _child;
        
        [SerializeField] private BoardModel _board;
        [SerializeField] private int _cardIndex;
        [SerializeField] private Vector2 _cardPosition =  new Vector2(0, 0);

        public bool IsEmpty { get; private set; }
        public CardModel Card => _child;

        public void Attach(CardModel card)
        {
            IsEmpty = false;
            _child = card;
            card.transform.SetParent(transform);
            MoveCardToPositionAsync(card.transform, _cts.Token).Forget();
        }

        public CardModel Detach()
        {
            var cardModel = _child;
            _child.transform.parent = null;
            _child = null;
            IsEmpty = false;
            return cardModel;
        }

        private async UniTask MoveCardToPositionAsync(Transform cardTransform, CancellationToken ct = default)
        {
            try
            {
                var moveDirection = cardTransform.position - transform.position;
                while (Vector2.Distance(cardTransform.position, _cardPosition) > CART_SNAP_DISTANCE)
                {
                    cardTransform.Translate(moveDirection * (Time.deltaTime * CART_MOVE_SPEED));
                    await UniTask.NextFrame(cancellationToken:ct);
                }

                cardTransform.position = _cardPosition; // Snap to final location
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Card Movement was canceled");
            }
        }

        private void OnDestroy() => _cts.Cancel();
    }
}