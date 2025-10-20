using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Other
{
    public class CardGroupLayout : MonoBehaviour
    {
        private const float SNAP_DISTANCE = 10f;
        private const int MIN_SORTING_ORDER = 3;
        [SerializeField] private Vector3 _rotation;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _spacing;
        private float _currentSpacing;
        private List<CardModel> _child = new List<CardModel>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private int _objectsChangingPosition;
        private bool _isChangingPosition;

        private void Awake()
        {
            _currentSpacing = _spacing;
            LoadChildren();
        }

        private void LoadChildren()
        {
            _child = transform.childCount > 0 
                ? GetComponentsInChildren<CardModel>().ToList() 
                : new List<CardModel>();
            UpdateChildPosition();
        }

        public void AddChild(CardModel child)
        {
            _child.Add(child);
            child.transform.SetParent(transform);
            child.transform.localScale = Vector3.one;
            child.transform.rotation = Quaternion.Euler(_rotation);
            UpdateChildPosition();
        }

        public void RemoveChild(int index)
        {
            _child[index].transform.rotation = Quaternion.identity;
            _child.RemoveAt(index);
            UpdateChildPosition();
        }
        
        private void UpdateChildPosition()
        {
            if (_isChangingPosition) _cts.Cancel();
            _isChangingPosition = true;
            _objectsChangingPosition = _child.Count;
            
            AdjustSpacing(out var totalWidth);

            var currentOrder = MIN_SORTING_ORDER;
            var currentPosition = -totalWidth / 2;
            for (var i = 0; i < _child.Count; i++)
            {
                _child[i].IndexInLayout = i;
                _child[i].SortingGroup.sortingOrder = currentOrder;
                
                var target = new Vector3(currentPosition, 0, 0);
                MoveRectAsync(_child[i].transform, target, _cts.Token).Forget();
                
                currentPosition += _spacing;
                currentOrder++;
            }
        }

        private void AdjustSpacing(out float totalWidth)
        {
            totalWidth = _child.Count * _spacing;
            
            if (totalWidth > Screen.width)
            {
                totalWidth = Screen.width;
                _currentSpacing = totalWidth / _child.Count;
            }
            else if (!Mathf.Approximately(_currentSpacing, _spacing))
            {
                _currentSpacing = _spacing;
            }
        }

        private async UniTask MoveRectAsync(Transform cardTransform, Vector3 final,
            CancellationToken ct)
        {
            var current = cardTransform.localPosition;
            while (Vector3.Distance(current, final) < SNAP_DISTANCE)
            {
                current = Vector3.Lerp(current, final, _moveSpeed);
                cardTransform.localPosition = current;
                await UniTask.NextFrame(cancellationToken: ct);
            }

            cardTransform.localPosition = final;

            _objectsChangingPosition--;
            if (_objectsChangingPosition <= 0)
            {
                _isChangingPosition = false;
            }
        }

        private void OnDestroy() => _cts.Cancel();
    }
}