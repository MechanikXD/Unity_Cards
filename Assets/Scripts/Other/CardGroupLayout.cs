using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Cards.Card;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Other.Extensions;
using UnityEngine;

namespace Other
{
    public class CardGroupLayout : MonoBehaviour
    {
        private const int MIN_SORTING_ORDER = 3;
        [SerializeField] private float _spacing;
        [SerializeField] private float _xOffset;
        [SerializeField] private Vector3 _rotation;
        [SerializeField] private float _moveSpeed;
        private float _currentSpacing;
        private List<CardModel> _child = new List<CardModel>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        
        private int _objectsChangingPositionCount;
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
            UpdateChildPosition(true);
        }

        public void AddChild(CardModel child)
        {
            _child.Add(child);
            child.transform.SetParent(transform);
            child.transform.rotation = Quaternion.Euler(_rotation);
            UpdateChildPosition();
        }

        public void RemoveChild(int index, [CanBeNull] Transform newParent=null)
        {
            _child[index].transform.rotation = Quaternion.identity;
            _child[index].transform.SetParent(newParent);
            _child.RemoveAt(index);
            UpdateChildPosition();
        }

        public void RemoveALl()
        {
            foreach (var card in _child) Destroy(card.gameObject);
            _child.Clear();
        }
        
        private void UpdateChildPosition(bool isInstant=false)
        {
            if (_isChangingPosition) _cts = _cts.Reset();
            _isChangingPosition = true;
            // Mark how many cards are moving so we know when to unflag _isChangingPosition
            _objectsChangingPositionCount = _child.Count;
            
            AdjustSpacing(out var totalWidth);

            var currentOrder = MIN_SORTING_ORDER;
            var currentPosition = -totalWidth / 2 + _xOffset;
            for (var i = 0; i < _child.Count; i++)
            {
                _child[i].IndexInLayout = i;
                _child[i].SortingGroup.sortingOrder = currentOrder;
                var target = new Vector3(currentPosition, 0, 0);
                
                if (isInstant) _child[i].transform.localPosition = target;
                else MoveChildAsync(_child[i].transform, target).Forget();
                
                currentPosition += _currentSpacing;
                currentOrder++;
            }

            if (isInstant)
            {
                _objectsChangingPositionCount = 0;
                _isChangingPosition = false;
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

        private async UniTask MoveChildAsync(Transform cardTransform, Vector3 final)
        {
            await cardTransform.MoveToLocalAsync(final, _moveSpeed, _cts.Token);

            _objectsChangingPositionCount--;
            if (_objectsChangingPositionCount <= 0)
            {
                _isChangingPosition = false;
            }
        }

        private void OnDestroy() => _cts.Cancel();
    }
}