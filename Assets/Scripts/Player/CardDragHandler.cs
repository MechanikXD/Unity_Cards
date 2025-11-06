using Core;
using Core.Cards.Board;
using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using Other;
using UnityEngine;

namespace Player
{
    public class CardDragHandler : DragHandler
    {
        private CardModel _thisModel;

        protected override void Awake()
        {
            _thisModel = GetComponent<CardModel>();
            base.Awake();
        }

        protected override void PickUpObject()
        {
            _thisModel.SortingGroup.sortingOrder = CardOrder;
            GameManager.Instance.Board.RemoveCardFromLayout(_thisModel.IndexInLayout);
        }

        protected override void DropObject()
        {
            if (_thisModel.CanBePlaced)
            {
                // BOARD MUST BE LOCATED ON XY PANE (position.z = 0, rotation = Vector3.zero).
                // Because we use transform.position, there is no z coordinate, so board can't have z as well.
                var hit = Physics2D.OverlapPoint(transform.position, _mouseReleaseMask);

                if (hit != null && hit.TryGetComponent<CardSlot>(out var slot) && slot.IsEmpty)
                {
                    slot.Attach(_thisModel);
                    _thisModel.SetPlaced();
                    Destroy(this);  // Make non-interactable
                    return;
                }
            }

            MoveToOriginalAsync().Forget();
        }
        
        private async UniTask MoveToOriginalAsync()
        {
            await transform.MoveToAsync(OriginalPosition, _cardMoveSpeed, this.GetCancellationTokenOnDestroy());
            transform.rotation = Quaternion.identity;
            GameManager.Instance.Board.AddCardToLayout(_thisModel);
        }
    }
}