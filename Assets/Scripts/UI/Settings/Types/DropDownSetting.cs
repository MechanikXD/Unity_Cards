using Structure;
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
            Title = settingName;
            _titleField.SetText(Title);

            foreach (var value in values)
            {
                _dropdown.options.Add(new TMP_Dropdown.OptionData(value));    
            }

            var optionIndex = 0;
            if (StorageProxy.HasSetting(Title))
            {
                var option = StorageProxy.GetSetting<string>(Title);
                for (var i = 0; i < _options.Length; i++)
                {
                    if (option != _options[i]) continue;

                    optionIndex = i;
                    break;
                }
            }
            
            _dropdown.value = optionIndex;
            CurrentOption = _options[optionIndex];
        }
        
        public override void WriteChangesInStorage() => StorageProxy.SetSetting(Title, CurrentOption);
    }
}