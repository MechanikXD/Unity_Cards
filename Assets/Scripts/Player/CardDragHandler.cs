using System.Threading;
using Core;
using Core.Cards.Board;
using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Player
{
    public class CardDragHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask _mouseReleaseMask;
        
        private const float CARD_SNAP_DISTANCE = 0.1f;
        private const float CARD_MOVE_SPEED = 10f;
        private const int CARD_ORDER = 99;
        
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Vector3 _originalPosition;
        private CardModel _thisModel;
        private Camera _camera;

        private void Awake()
        {
            _thisModel = GetComponent<CardModel>();
            if (Camera.main == null)
            {
                Debug.LogError("There is no main camera on this scene!");
            }
            else _camera = Camera.main;
        }

        public void OnMouseDown()
        {
            if (!GlobalInputBlocker.Instance.InputEnabled) return;

            _originalPosition = transform.position;
            _thisModel.SortingGroup.sortingOrder = CARD_ORDER;
            GameManager.Instance.Board.RemoveCardFromLayout(_thisModel.IndexInLayout);
        }

        public void OnMouseDrag()
        {
            transform.position = GetRaycastHitPoint();
        }

        public void OnMouseUp()
        {
            var cardPos2D = new Vector2(transform.position.x, transform.position.y);
            // Check for overlapping slot collider
            var hit = Physics2D.OverlapPoint(cardPos2D, _mouseReleaseMask);

            if (hit != null)
            {
                var slot = hit.GetComponent<CardSlot>();
                if (slot != null && slot.IsEmpty && _thisModel.CanBePlaced)
                {
                    _thisModel.transform.SetParent(slot.transform);
                    slot.Attach(_thisModel);
                    _thisModel.SetPlaced();
                    Destroy(this);
                    return;
                }
            }

            MoveToOriginalAsync(_cts.Token).Forget();
        }

        private Vector3 GetRaycastHitPoint()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var boardPlane = new Plane(Vector3.forward, Vector3.zero);
            return boardPlane.Raycast(ray, out var enter) 
                ? ray.GetPoint(enter) : Vector3.zero;
        }

        private void OnDestroy()
        {
            _cts.Cancel();
        }

        private async UniTask MoveToOriginalAsync(CancellationToken ct = default)
        {
            var moveDirection = _originalPosition - transform.position;
            while (Vector2.Distance(transform.position, _originalPosition) > CARD_SNAP_DISTANCE)
            {
                transform.Translate(moveDirection * (Time.deltaTime * CARD_MOVE_SPEED));
                await UniTask.NextFrame(cancellationToken:ct);
            }
            transform.position = _originalPosition;
            
            GameManager.Instance.Board.AddCardToLayout(_thisModel);
        }
    }
}