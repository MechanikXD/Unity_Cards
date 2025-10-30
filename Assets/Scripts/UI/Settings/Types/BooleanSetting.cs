using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings.Types
{
    public class BooleanSetting : Setting
    {
        [SerializeField] private Toggle _switch;
        private string _name;
        private bool _defaultValue;
        public bool IsOn { get; private set; }

        private void OnEnable()
        {
            _switch.onValueChanged.AddListener(OnSwitch);
        }
        
        private void OnDisable()
        {
            _switch.onValueChanged.RemoveListener(OnSwitch);
        }

        private void OnSwitch(bool isOn)
        {
            IsOn = isOn;
            SettingsChanged();
        }

        private void Awake()
        {
            SetValues();
        }

        public void Load(string settingName, bool value)
        {
            _title.SetText(settingName);
            _defaultValue = value;
            IsOn = value;
        }
        
        public override void SetValues()
        {
            _switch.isOn = _defaultValue;
        }
    }
}