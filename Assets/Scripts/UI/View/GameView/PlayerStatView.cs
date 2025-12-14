using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Other.Interactions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View.GameView
{
    public class PlayerStatView : MonoBehaviour
    {
        private const float VALUE_CHANGE_SPEED = 0.75f;
        [SerializeField, CanBeNull] private HighLightRed _highLight;
        
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private Image _healthFill;
        private float _targetHealthFill = 1f;
        private bool _currentlyChangingHealthFill;
        
        [SerializeField] private TMP_Text _lightText;
        [SerializeField] private Image _lightFill;
        private float _targetLightFill = 1f;
        private bool _currentlyChangingLightFill;

        public void HighLight() => _highLight?.HighLight();

        public void SetHealth(int health, int maxHealth, bool instantChange=false)
        {
            _healthText.SetText(health.ToString());
            _targetHealthFill = health / (float)maxHealth;
            if (instantChange) _healthFill.fillAmount = _targetHealthFill;
            else if (!_currentlyChangingHealthFill) ChangeHealthFillAmountAsync().Forget();
        }
        
        public void SetLight(int currentLight, int maxLight, bool instantChange=false)
        {
            _lightText.SetText(currentLight.ToString());
            _targetLightFill = currentLight / (float)maxLight;
            if (instantChange) _lightFill.fillAmount = _targetLightFill;
            else if (!_currentlyChangingLightFill) ChangeLightFillAmountAsync().Forget();
        }

        private async UniTask ChangeHealthFillAmountAsync()
        {
            _currentlyChangingHealthFill = true;
            
            var currentFillValue = _healthFill.fillAmount;
            while (!Mathf.Approximately(currentFillValue, _targetHealthFill))
            {
                var valueChangeThisFrame = VALUE_CHANGE_SPEED * Time.deltaTime;
                if (Mathf.Abs(currentFillValue - _targetHealthFill) < valueChangeThisFrame) 
                    currentFillValue = _targetHealthFill;
                else
                    currentFillValue += currentFillValue > _targetHealthFill
                        ? -valueChangeThisFrame
                        : valueChangeThisFrame;
                
                _healthFill.fillAmount = currentFillValue;
                await UniTask.NextFrame(destroyCancellationToken);
            }
            
            _currentlyChangingHealthFill = false;
        }

        private async UniTask ChangeLightFillAmountAsync()
        {
            _currentlyChangingLightFill = true;
            
            var currentFillValue = _lightFill.fillAmount;
            while (!Mathf.Approximately(currentFillValue, _targetLightFill))
            {
                var valueChangeThisFrame = VALUE_CHANGE_SPEED * Time.deltaTime;
                if (Mathf.Abs(currentFillValue - _targetLightFill) < valueChangeThisFrame) 
                    currentFillValue = _targetLightFill;
                else currentFillValue += currentFillValue > _targetLightFill
                    ? -valueChangeThisFrame
                    : valueChangeThisFrame;
                
                _lightFill.fillAmount = currentFillValue;
                await UniTask.NextFrame(destroyCancellationToken);
            }
            
            _currentlyChangingLightFill = false;
        }
    }
}