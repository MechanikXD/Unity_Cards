using Core.Cards.Deck;
using Cysharp.Threading.Tasks;
using Other;
using UI;
using UI.View.MainMenuView;
using UnityEngine;

namespace Player.Deck
{
    public class DeckDragHandler : DragHandler
    {
        private DeckCardModel _thisModel;

        protected override void Awake()
        {
            _thisModel = GetComponent<DeckCardModel>();
            base.Awake();
        }

        protected override void PickUpObject()
        {
            _thisModel.SortingGroup.sortingOrder = CardOrder;
        }

        protected override void DropObject()
        {
            // BOARD MUST BE LOCATED ON XY PANE (position.z = 0, rotation = Vector3.zero).
            // Because we use transform.position, there is no z coordinate, so board can't have z as well.
            var hit = Physics2D.OverlapPoint(transform.position, _mouseReleaseMask);
            if (hit == null) return;
                    
            if (hit.TryGetComponent<PlayerCardDragArea>(out var playerArea) && playerArea.CanAddToDeck(_thisModel.CardData.Cost) 
                && !_thisModel.InPlayerHand)
            {
                UIManager.Instance.GetUICanvas<DeckView>().OtherCards
                    .RemoveCard(_thisModel.IndexInLayout);
                playerArea.AddCard(_thisModel);
            }
            else if (hit.TryGetComponent<OtherCardDragArea>(out var otherArea) &&
                     _thisModel.InPlayerHand)
            {
                UIManager.Instance.GetUICanvas<DeckView>().PlayerCards
                    .RemoveCard(_thisModel.IndexInLayout);
                otherArea.AddCard(_thisModel);
            }
            else MoveToOriginalAsync().Forget();
        }

        private async UniTask MoveToOriginalAsync()
        {
            await transform.MoveToAsync(OriginalPosition, _cardMoveSpeed, this.GetCancellationTokenOnDestroy());
            transform.rotation = Quaternion.identity;
        }
    }
}