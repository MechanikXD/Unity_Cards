using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Settings
{
    public abstract class Setting : MonoBehaviour
    {
        [FormerlySerializedAs("_title"),SerializeField] protected TMP_Text _titleField;
        protected string Title;
        public bool WasChanged { get; private set; }
        public void ClearChanged() => WasChanged = false;
        public abstract void WriteChangesInStorage();
        // Event that invokes all changes.
        public event Action<Setting> OnSettingChanged;
        // Method to invoke event. Called within settings when value changed.
        internal void SettingsChanged()
        {
            OnSettingChanged?.Invoke(this);
            WasChanged = true;
        }
    }
}