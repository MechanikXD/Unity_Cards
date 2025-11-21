using Core;
using Core.Cards.Board;
using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using Other;
using UnityEngine;

namespace Player
{
    public class CardDragHandler : MonoBehaviour
    {
        private const int CARD_ORDER = 99;
        [SerializeField] private LayerMask _mouseReleaseMask;
        [SerializeField] private float _cardMoveSpeed = 2f;
        [SerializeField] private Vector2 _xMoveBorders;
        
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
        
        public void OnMouseDown()
        {
            if (!GlobalInputBlocker.Instance.InputEnabled) return;

            _originalPosition = transform.position;
            _thisModel.SortingGroup.sortingOrder = CARD_ORDER;
            GameManager.Instance.Board.RemoveCardFromLayout(_thisModel.IndexInLayout);
        }

        public void OnMouseDrag()
        {
            var newPoint = GetRaycastHitPoint();
            newPoint.x = Mathf.Clamp(newPoint.x, _xMoveBorders.x, _xMoveBorders.y);
            transform.position = newPoint;
        }

        public void OnMouseUp()
        {
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