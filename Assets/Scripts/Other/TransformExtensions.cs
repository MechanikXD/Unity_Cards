using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Other
{
    public static class TransformExtensions
    {
        public const float SnapDistance = 0.1f;
        
        public async static UniTask MoveToLocalAsync(this Transform transform, Vector3 final,
            float moveSpeed, CancellationToken ct)
        {
            var current = transform.localPosition;
            while (Vector3.Distance(current, final) > SnapDistance)
            {
                current = Vector3.Lerp(current, final, moveSpeed * Time.deltaTime);
                transform.localPosition = current;
                await UniTask.NextFrame(cancellationToken: ct);
            }

            transform.localPosition = final;
        }
        
        public async static UniTask MoveToAsync(this Transform transform, Vector3 final,
            float moveSpeed, CancellationToken ct)
        {
            var current = transform.position;
            while (Vector3.Distance(current, final) > SnapDistance)
            {
                current = Vector3.Lerp(current, final, moveSpeed * Time.deltaTime);
                transform.position = current;
                await UniTask.NextFrame(cancellationToken: ct);
            }

            transform.position = final;
        }
    }
}