using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings.Types
{
    public class SliderSetting : Setting
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_InputField _inputField;
        
        private float _defaultValue;
        private Vector2 _bounds;
        
        public float CurrentValue { get; private set; }

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(UpdateInputValue);
            _inputField.onValueChanged.AddListener(UpdateSliderValue);
        }
        
        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(UpdateInputValue);
            _inputField.onValueChanged.RemoveListener(UpdateSliderValue);
        }

        public void Load(string settingName, float value, Vector2 bounds)
        {
            _title.SetText(settingName);
            _defaultValue = value;
            _bounds = bounds;
            
            _slider.minValue = _bounds.x;
            _slider.maxValue = _bounds.y;
            
            _slider.value = Mathf.Clamp(_defaultValue, _slider.minValue, _slider.maxValue);
            _inputField.text = _defaultValue.ToString(CultureInfo.InvariantCulture);
            CurrentValue = _slider.value;
        }

        private void UpdateSliderValue(string inputValue)
        {
            // TODO: Fix number format and cut to X.XX
            var value = float.Parse(inputValue, NumberStyles.Any, CultureInfo.InvariantCulture);
            value = Mathf.Clamp(value, _bounds.x, _bounds.y);
            
            _slider.value = value;
            UpdateInputValue(value);
        }

        private void UpdateInputValue(float sliderValue)
        {
            _inputField.text = sliderValue.ToString(CultureInfo.InvariantCulture);
            CurrentValue = sliderValue;
            SettingsChanged();
        }
    }
}