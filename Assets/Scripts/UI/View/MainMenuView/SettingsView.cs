using System;
using System.Collections.Generic;
using Storage;
using UI.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View.MainMenuView
{
    public class SettingsView : CanvasView
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private SettingGroupTab _groupTabPrefab;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private Transform _groupTabRoot;
        [SerializeField] private SettingsData _settingData;
        private readonly static Dictionary<string, Setting> Settings = new Dictionary<string, Setting>();
        
        private readonly List<SettingGroupTab> _groupTabs = new List<SettingGroupTab>();
        private int _currentGroupIndex;

        protected override void Awake()
        {
            base.Awake();
            LoadGroups();
        }

        public static void AddEventOnSetting(string settingName, Action<Setting> action)
        {
            if (Settings.TryGetValue(settingName, out var setting))
            {
                setting.OnSettingChanged += action;
            }
            else
            {
                Debug.LogError($"{settingName} does not exist in setting");
            }
        }
        
        public void SwitchGroup(int index)
        {
            if (index == _currentGroupIndex) return;
            
            _groupTabs[_currentGroupIndex].HideGroup();
            _currentGroupIndex = index;
            _groupTabs[_currentGroupIndex].ShowGroup();
        }

        private void SaveChangedSettings()
        {
            foreach (var settingGroupTab in _groupTabs)
            {
                foreach (var changedSetting in settingGroupTab.GetChangedSettings())
                {
                    changedSetting.WriteChangesInStorage();
                }
            }
            
            StorageProxy.SaveSettings();
        }

        private void LoadGroups()
        {
            var firstDisplayed = false;
            for (var i = 0; i < _settingData.SettingGroups.Length; i++)
            {
                // Create new group tab
                var newGroup = Instantiate(_groupTabPrefab, _groupTabRoot);
                newGroup.SetRoot(_contentRoot, i);
                _groupTabs.Add(newGroup);

                // Create settings for this group
                newGroup.LoadSettings(_settingData.SettingGroups[i], out var createdSettings);
                // Load settings into dictionary
                foreach (var setting in createdSettings) Settings.Add(setting.Key, setting.Value);
                
                // Enable only first tab
                if (!firstDisplayed)
                {
                    newGroup.ShowGroup();
                    _currentGroupIndex = i;
                    firstDisplayed = true;
                }
                else newGroup.HideGroup();
            }
        }
        
        private void OnEnable()
        {
            _backButton.onClick.AddListener(ExitUICanvas);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(ExitUICanvas);
        }

        private void ExitUICanvas()
        {
            SaveChangedSettings();
            if (UIManager.Instance != null) UIManager.Instance.ExitLastCanvas();
        }
    }
}