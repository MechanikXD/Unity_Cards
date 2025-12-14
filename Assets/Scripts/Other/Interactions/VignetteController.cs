using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Other.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Other.Interactions
{
    public class VignetteController : MonoBehaviour
    {
        [SerializeField] private VolumeProfile _volumeProfile;
        [SerializeField] private float _fadeInSpeed = 25f;
        [SerializeField] private float _fadeOutSpeed = 15f;
        private Vignette _vignette;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private void Awake()
        {
            if (!_volumeProfile.TryGet(out _vignette))
            {
                Debug.LogError("VignetteController: VolumeProfile has no Vignette.", this);
                return;
            }

            _vignette.intensity.Override(0f);
        }

        private void OnDestroy()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        public void PulseVignette(float targetIntensity)
        {
            _cts = _cts.Reset();
            AnimateVignetteAsync(targetIntensity).Forget();
        }

        private async UniTask AnimateVignetteAsync(float target)
        {
            try
            {
                await LerpIntensity(target, _fadeInSpeed);
                await LerpIntensity(0f, _fadeOutSpeed);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            finally
            {
                // Ensure valid final state
                if (!_cts.Token.IsCancellationRequested)
                    _vignette.intensity.Override(0f);
            }
        }

        private async UniTask LerpIntensity(float target, float speed)
        {
            while (Mathf.Abs(_vignette.intensity.value - target) > 0.01f)
            {
                _vignette.intensity.Override(Mathf.Lerp(_vignette.intensity.value, target, speed * Time.deltaTime));
                await UniTask.NextFrame(_cts.Token);
            }

            _vignette.intensity.Override(target);
        }
    }
}