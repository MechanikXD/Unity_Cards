using Core;
using Core.Cards.Board;
using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using Other;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player
{
    public class CardController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        private const int CARD_ORDER = 99;
        [SerializeField] private LayerMask _mouseReleaseMask;
        [SerializeField] private float _cardMoveSpeed = 2f;
        [SerializeField] private Vector2 _xMoveBorders;
        [SerializeField] private float _holdThreshold = 0.3f;

        private float _lastPointerDownTime;
        private bool _isDrag;
        private Vector3 _originalPosition;
        private Plane _plane = new Plane(Vector3.forward, Vector3.zero);
        private Camera _camera;
        private CardModel _thisModel;

        protected void Awake()
        {
            _thisModel = GetComponent<CardModel>();
            if (Camera.main == null)
            {
                Debug.LogError("There is no main camera on this scene!");
            }
            else _camera = Camera.main;
        }

        // Must be implemented fot PointerUp event.
        public void OnPointerDown(PointerEventData eventData)
        {
            _lastPointerDownTime = Time.unscaledTime;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isDrag) return;
            var duration = Time.unscaledTime - _lastPointerDownTime;
            
            if (duration >= _holdThreshold)
            {
                // TODO: display details
            }
            else
            {
                // TODO: Show actions
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!GlobalInputBlocker.Instance.InputEnabled) return;

            _isDrag = true;
            _originalPosition = transform.position;
            _thisModel.SortingGroup.sortingOrder = CARD_ORDER;
            GameManager.Instance.Board.RemoveCardFromLayout(_thisModel.IndexInLayout);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var newPoint = GetRaycastHitPoint();
            newPoint.x = Mathf.Clamp(newPoint.x, _xMoveBorders.x, _xMoveBorders.y);
            transform.position = newPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDrag = false;
            if (_thisModel.CanBePlaced)
            {
                // BOARD MUST BE LOCATED ON XY PANE (position.z = 0, rotation = Vector3.zero).
                // Because we use transform.position, there is no z coordinate, so board can't have z as well.
                var hit = Physics2D.OverlapPoint(transform.position, _mouseReleaseMask);

                if (hit != null && hit.TryGetComponent<CardSlot>(out var slot) && slot.IsEmpty && slot.CanAttach)
                {
                    slot.Attach(_thisModel);
                    _thisModel.SetPlaced();
                    Destroy(this);  // Make non-interactable
                    return;
                }
            }

            MoveToOriginalAsync().Forget();
        }

        private Vector3 GetRaycastHitPoint()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            return _plane.Raycast(ray, out var intersection) 
                ? ray.GetPoint(intersection) : Vector3.zero;
        }
        
        private async UniTask MoveToOriginalAsync()
        {
            await transform.MoveToAsync(_originalPosition, _cardMoveSpeed, this.GetCancellationTokenOnDestroy());
            transform.rotation = Quaternion.identity;
            GameManager.Instance.Board.AddCardToLayout(_thisModel);
        }
    }
}