using Cards.Board;
using Cysharp.Threading.Tasks;
using Other.Extensions;
using Other.Interactions;
using Structure.Managers;
using UI;
using UI.View.GameView;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cards.Card
{
    public class CardController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        private const int CARD_ORDER = 99;
        [SerializeField] private LayerMask _mouseReleaseMask;
        [SerializeField] private float _cardMoveSpeed = 2f;
        [SerializeField] private Vector2 _xMoveBorders;
        [SerializeField] private float _holdThreshold = 0.3f;

        public bool Interactable { get; set; } = true;
        private float _lastPointerDownTime;
        private bool _isDrag;
        private bool _isPlaced;
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

        // Must be implemented fot PointerUp event. (And correct duration handling)
        public void OnPointerDown(PointerEventData eventData)
        {
            _lastPointerDownTime = Time.unscaledTime;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isDrag)
            {
                HideInfoOnClick.HideAll();
                return;
            }
            var duration = Time.unscaledTime - _lastPointerDownTime;
            
            if (duration >= _holdThreshold)
            {
                HideInfoOnClick.CancelCardMove();
                var detailView = UIManager.Instance.GetHUDCanvas<CardDetailView>();
                detailView.LoadData(_thisModel);
                detailView.Enable();
            }
            else if (Interactable && _thisModel.Hand == null)
            {
                HideInfoOnClick.HideAll();
                _thisModel.ShowActions();
            }
            else
            {
                HideInfoOnClick.HideAll();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isPlaced || !Interactable) return;
            HideInfoOnClick.HideAll();

            _isDrag = true;
            _originalPosition = transform.position;
            _thisModel.SortingGroup.sortingOrder = CARD_ORDER;
            GameManager.Instance.Board.RemoveCardFromLayout(_thisModel.IndexInLayout);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isPlaced || !Interactable) return;
            var newPoint = GetRaycastHitPoint();
            newPoint.x = Mathf.Clamp(newPoint.x, _xMoveBorders.x, _xMoveBorders.y);
            transform.position = newPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isPlaced || !Interactable) return;
            _isDrag = false;
            if (_thisModel.CanBePlaced)
            {
                // BOARD MUST BE LOCATED ON XY PANE (position.z = 0, rotation = Vector3.zero).
                // Because we use transform.position, there is no z coordinate, so board can't have z as well.
                var hit = Physics2D.OverlapPoint(transform.position, _mouseReleaseMask);

                if (hit != null && hit.TryGetComponent<CardSlot>(out var slot) && slot.IsEmpty && slot.CanSnapTo)
                {
                    slot.Attach(_thisModel);
                    _thisModel.SetPlaced();
                    _isPlaced = true;
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