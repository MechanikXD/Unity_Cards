using System;
using TMPro;
using UnityEngine;

namespace UI.Settings
{
    public abstract class Setting : MonoBehaviour
    {
        [SerializeField] protected TMP_Text _title;

        private void Start() => SettingsChanged();

        public abstract void SetValues();
        
        public event Action<Setting> OnSettingChanged;
        
        protected void SettingsChanged() => OnSettingChanged?.Invoke(this);
    }
}