using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View
{
    public class PlayerStatView : MonoBehaviour
    {
        private const float VALUE_CHANGE_SPEED = 0.05f;
        private readonly CancellationTokenSource _cts = new  CancellationTokenSource();
        
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private Image _healthFill;
        private float _targetHealthFill = 1f;
        private bool _currentlyChangingHealthFill;
        
        [SerializeField] private TMP_Text _hopeText;
        [SerializeField] private Image _hopeFill;
        private float _targetHopeFill = 1f;
        private bool _currentlyChangingHopeFill;

        public void SetHealth(int health, int maxHealth, bool instantChange=false)
        {
            _healthText.SetText(health.ToString());
            _targetHealthFill = health / (float)maxHealth;
            if (instantChange) _healthFill.fillAmount = _targetHealthFill;
            else if (!_currentlyChangingHealthFill) ChangeHealthFillAmountAsync(_cts.Token).Forget();
        }
        
        public void SetHope(int hope, int maxHope, bool instantChange=false)
        {
            _hopeText.SetText(hope.ToString());
            _targetHopeFill = hope / (float)maxHope;
            if (instantChange) _hopeFill.fillAmount = _targetHopeFill;
            else if (!_currentlyChangingHopeFill) ChangeHopeFillAmountAsync(_cts.Token).Forget();
        }

        private async UniTask ChangeHealthFillAmountAsync(CancellationToken ct = default)
        {
            _currentlyChangingHealthFill = true;
            
            var currentFillValue = _healthFill.fillAmount;
            while (!Mathf.Approximately(currentFillValue, _targetHealthFill))
            {
                if (Mathf.Abs(currentFillValue - _targetHealthFill) < VALUE_CHANGE_SPEED) 
                    currentFillValue = _targetHealthFill;
                else
                    currentFillValue += currentFillValue > _targetHealthFill
                        ? -VALUE_CHANGE_SPEED
                        : VALUE_CHANGE_SPEED;
                
                _healthFill.fillAmount = currentFillValue;
                await UniTask.NextFrame(ct);
            }
            
            _currentlyChangingHealthFill = false;
        }

        private async UniTask ChangeHopeFillAmountAsync(CancellationToken ct = default)
        {
            _currentlyChangingHopeFill = true;
            
            var currentFillValue = _hopeFill.fillAmount;
            while (!Mathf.Approximately(currentFillValue, _targetHopeFill))
            {
                if (Mathf.Abs(currentFillValue - _targetHopeFill) < VALUE_CHANGE_SPEED) 
                    currentFillValue = _targetHopeFill;
                else currentFillValue += currentFillValue > _targetHopeFill
                    ? -VALUE_CHANGE_SPEED
                    : VALUE_CHANGE_SPEED;
                
                _hopeFill.fillAmount = currentFillValue;
                await UniTask.NextFrame(ct);
            }
            
            _currentlyChangingHopeFill = false;
        }

        private void OnDisable()
        {
            _cts.Cancel();
        }
    }
}