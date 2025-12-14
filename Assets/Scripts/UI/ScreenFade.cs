using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Structure.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScreenFade : CanvasView
    {
        private const float EPSI = 0.05f;
        [SerializeField] private float _fadeSpeed;
        [SerializeField] private Image _fadeScreen;
        private bool _changingAlpha;

        private void Start() => FadeOut();

        private void FadeOut()
        {
            if (!_changingAlpha) ChangeImageAlphaAsync(0f).Forget();
        }
        
        public void FadeIn(Action after)
        {
            if (!_changingAlpha) ChangeImageAlphaAsync(1f, after).Forget();
        }

        private async UniTask ChangeImageAlphaAsync(float target, [CanBeNull] Action afterAction=null)
        {
            var task = AudioManager.Instance.StopAllSources();
            _changingAlpha = true;
            GlobalInputBlocker.Instance.DisableInput();
            var baseColor = _fadeScreen.color;
            var currentAlpha = baseColor.a;
            while (Mathf.Abs(currentAlpha - target) > EPSI)
            {
                currentAlpha = Mathf.Lerp(currentAlpha, target, _fadeSpeed * Time.deltaTime);
                baseColor.a = currentAlpha;
                _fadeScreen.color = baseColor;
                await UniTask.NextFrame(destroyCancellationToken);
            }

            baseColor.a = target;
            _fadeScreen.color = baseColor;
            GlobalInputBlocker.Instance.EnableInput();
            _changingAlpha = false;
            await task;
            afterAction?.Invoke();
        }
    }
}