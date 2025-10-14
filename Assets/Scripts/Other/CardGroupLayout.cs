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
        [SerializeField] private Vector3 _rotation;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _spacing;
        private float _currentSpacing;
        private List<RectTransform> _child = new List<RectTransform>();
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
            _child = transform.childCount == 0
                ? new List<RectTransform>()
                : gameObject.transform.Cast<RectTransform>().ToList();
            UpdateChildPosition();
        }

        public void AddChild(RectTransform child)
        {
            _child.Add(child);
            child.transform.SetParent(transform);
            child.localScale = Vector3.one;
            child.rotation = Quaternion.Euler(_rotation);
            UpdateChildPosition();
        }

        public void RemoveChild(int index)
        {
            _child[index].rotation = Quaternion.identity;
            _child.RemoveAt(index);
            UpdateChildPosition();
        }
        
        private void UpdateChildPosition()
        {
            if (_isChangingPosition) _cts.Cancel();
            _isChangingPosition = true;
            _objectsChangingPosition = _child.Count;
            
            AdjustSpacing(out var totalWidth);
            
            var currentPosition = -totalWidth / 2;
            for (var i = 0; i < _child.Count; i++)
            {
                _child[i].GetComponent<CardModel>().IndexInLayout = i;
                
                _child[i] .SetSiblingIndex(i);
                MoveRectAsync(_child[i], new Vector2(currentPosition, 
                    _child[i].anchoredPosition.y), _cts.Token).Forget();
                currentPosition += _spacing;
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

        private async UniTask MoveRectAsync(RectTransform rectTransform, Vector2 final,
            CancellationToken ct)
        {
            var current = rectTransform.anchoredPosition;
            while (Vector2.Distance(current, final) < SNAP_DISTANCE)
            {
                current = Vector2.Lerp(current, final, _moveSpeed);
                rectTransform.anchoredPosition = current;
                await UniTask.NextFrame(cancellationToken: ct);
            }

            rectTransform.anchoredPosition = final;

            _objectsChangingPosition--;
            if (_objectsChangingPosition <= 0)
            {
                _isChangingPosition = false;
            }
        }

        private void OnDestroy() => _cts.Cancel();
    }
}