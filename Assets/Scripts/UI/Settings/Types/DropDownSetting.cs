using TMPro;
using UnityEngine;

namespace UI.Settings.Types
{
    public class DropDownSetting : Setting
    {
        [SerializeField] TMP_Dropdown _dropdown;
        private string[] _options;
        public string CurrentOption { get; private set; }

        private void OnEnable()
        {
            _dropdown.onValueChanged.AddListener(OnSwitch);
        }
        
        private void OnDisable()
        {
            _dropdown.onValueChanged.RemoveListener(OnSwitch);
        }

        private void OnSwitch(int option)
        {
            CurrentOption = _options[option];
            SettingsChanged();
        }

        public void Load(string settingName, string[] values)
        {
            _options = values;
            _title.SetText(settingName);

            foreach (var value in values)
            {
                _dropdown.options.Add(new TMP_Dropdown.OptionData(value));    
            }
            
            _dropdown.value = 0;
            CurrentOption = _options[0];
        }
    }
}