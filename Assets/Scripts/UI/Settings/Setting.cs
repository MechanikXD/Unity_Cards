using System;
using TMPro;
using UnityEngine;

namespace UI.Settings
{
    public abstract class Setting : MonoBehaviour
    {
        [SerializeField] protected TMP_Text _title;
        // Initial call to set up changed/default values.
        private void Start() => SettingsChanged();
        // Event that invokes all changes.
        public event Action<Setting> OnSettingChanged;
        // Method to invoke event. Called within settings when value changed.
        protected void SettingsChanged() => OnSettingChanged?.Invoke(this);
    }
}