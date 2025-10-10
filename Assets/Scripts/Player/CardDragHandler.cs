using System.Threading;
using Core;
using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player
{
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const float CARD_SNAP_DISTANCE = 10f;
        private const float CARD_MOVE_SPEED = 5f;
        
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        // TODO: Pass this with zenject
        private Transform _relocationCanvas;
        private CanvasGroup _canvasGroup;
        private Vector3 _originalPosition;
        private Transform _originalParent;
        private CardModel _thisModel;

        private void Awake()
        {
            _thisModel = GetComponent<CardModel>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            _relocationCanvas = GameManager.Instance.ControlCanvas;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalPosition = transform.position;
            _originalParent = transform.parent;
            _canvasGroup.blocksRaycasts = false;
            transform.SetParent(_relocationCanvas);
            transform.SetAsFirstSibling();
            
            GameManager.Instance.Board.RemoveCardFromLayout(_thisModel.IndexInLayout);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            if (transform.parent == _relocationCanvas)
            {
                MoveToOriginalAsync(_cts.Token).Forget();
            }
            
            else DestroyImmediate(this);
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

            transform.position = _originalPosition; // Snap to final location
            transform.SetParent(_originalParent);
            
            GameManager.Instance.Board.AddCardToLayout((RectTransform)_thisModel.transform);
        }
    }
}