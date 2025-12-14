using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Other.Interactions
{
    public class HighLightRed : MonoBehaviour
    {
        [SerializeField] private MaskableGraphic _target;
        [SerializeField] private float _valueSnap = 10f;
        [SerializeField] private float _enterSpeed = 20f;
        [SerializeField] private float _stayTime = 0.5f;
        [SerializeField] private float _exitSpeed = 5f;
        private bool _highlighting;
        
        public void HighLight()
        {
            if (!_highlighting) HighLightRedAsync().Forget();
        }

        private async UniTask HighLightRedAsync()
        {
            _highlighting = true;
            var originalColor = _target.color;
            while (ColorDistance(_target.color, Color.red) > _valueSnap)
            {
                _target.color = Color.Lerp(_target.color, Color.red, _enterSpeed * Time.deltaTime);
                await UniTask.NextFrame(this.destroyCancellationToken);
            }

            _target.color = Color.red;
            await UniTask.WaitForSeconds(_stayTime, cancellationToken:this.destroyCancellationToken);
            while (ColorDistance(_target.color, originalColor) > _valueSnap)
            {
                _target.color = Color.Lerp(_target.color, originalColor, _exitSpeed * Time.deltaTime);
                await UniTask.NextFrame(this.destroyCancellationToken);
            }

            _target.color = originalColor;
            _highlighting = false;
        }

        private static float ColorDistance(Color first, Color second)
        {
            return Mathf.Abs(first.r - second.r) + Mathf.Abs(first.g - second.g) +
                   Mathf.Abs(first.b - second.b);
        }
    }
}