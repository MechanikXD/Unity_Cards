using System.Globalization;
using Structure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings.Types
{
    public class SliderSetting : Setting
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_InputField _inputField;
        
        private bool _wholeNumbers;
        private float _defaultValue;
        private Vector2 _bounds;
        
        public float CurrentValue { get; private set; }

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(UpdateInputValue);
            _inputField.onEndEdit.AddListener(UpdateSliderValue);
        }
        
        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(UpdateInputValue);
            _inputField.onEndEdit.RemoveListener(UpdateSliderValue);
        }

        public void Load(string settingName, float value, Vector2 bounds, bool wholeNumbers)
        {
            _titleField.SetText(settingName);
            Title = settingName;
            _bounds = bounds;
            _defaultValue = Mathf.Clamp(value, _bounds.x, _bounds.y);
            _wholeNumbers = wholeNumbers;
            
            _slider.minValue = _bounds.x;
            _slider.maxValue = _bounds.y;
            _slider.wholeNumbers = wholeNumbers;
            
            CurrentValue = StorageProxy.HasSetting(Title) ? StorageProxy.GetSetting<float>(Title) : _defaultValue;
            _slider.SetValueWithoutNotify(CurrentValue);
            _inputField.SetTextWithoutNotify(CurrentValue.ToString(CultureInfo.InvariantCulture));
        }

        private void UpdateSliderValue(string inputValue)
        {
            inputValue = inputValue.Replace(",", ".");
            var value = float.Parse(inputValue, CultureInfo.InvariantCulture);
            value = Mathf.Clamp(value, _bounds.x, _bounds.y);
            if (_wholeNumbers) value = (int)(value + 0.999f);
            
            _slider.SetValueWithoutNotify(value);
            UpdateInputValue(value);
        }

        private void UpdateInputValue(float sliderValue)
        {
            _inputField.SetTextWithoutNotify(sliderValue.ToString("F"));
            CurrentValue = sliderValue;
            SettingsChanged();
        }

        public override void WriteChangesInStorage() => StorageProxy.SetSetting(_titleField.text, CurrentValue);
    }
}