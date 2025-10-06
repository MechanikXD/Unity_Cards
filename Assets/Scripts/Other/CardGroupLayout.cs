using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Other
{
    public class CardGroupLayout : MonoBehaviour
    {
        private const float SNAP_DISTANCE = 10f;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _spacing;
        private List<RectTransform> _child = new List<RectTransform>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private void Awake()
        {
            _child = GetChild();
            UpdateChildPosition();
        }

        private void AddChild(RectTransform child)
        {
            _child.Add(child);
            UpdateChildPosition();
        }

        private void RemoveChild(int index)
        {
            _child.RemoveAt(index);
            UpdateChildPosition();
        }
        
        private void UpdateChildPosition()
        {
            var totalWidth = _child.Count * _spacing;
            var currentPosition = -totalWidth / 2;
            foreach (var child in _child)
            {
                MoveRectAsync(child, new Vector2(currentPosition, child.localPosition.y),
                    _moveSpeed, _cts.Token).Forget();
                currentPosition += _spacing;
            }
        }

        private async static UniTask MoveRectAsync(RectTransform rectTransform, Vector2 final, float moveSpeed, CancellationToken ct)
        {
            var current = rectTransform.localPosition;
            while (Vector2.Distance(current, final) < SNAP_DISTANCE)
            {
                current = Vector2.Lerp(current, final, moveSpeed);
                rectTransform.localPosition = current;
                await UniTask.NextFrame(cancellationToken:ct);
            }
            
            rectTransform.localPosition = final;
        }

        private List<RectTransform> GetChild()
        {
            return transform.childCount == 0 ? new  List<RectTransform>() 
                : transform.Cast<RectTransform>().ToList();
        }

        private void OnDestroy()
        {
            _cts.Cancel();
        }
    }
}