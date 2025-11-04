using System.Collections.Generic;
using TMPro;
using UI.View.MainMenuView;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class SettingGroupTab : MonoBehaviour
    {
        [SerializeField] private Button _groupButton;
        [SerializeField] private TMP_Text _title;
        private Transform _content;
        private readonly List<Setting> _loadedSettings = new List<Setting>();
        public int GroupIndex { get; private set; }

        private void OnEnable()
        {
            _groupButton.onClick.AddListener(SwitchToThisGroup);
        }

        private void OnDisable()
        {
            _groupButton.onClick.RemoveListener(SwitchToThisGroup);
        }

        private void SwitchToThisGroup()
        {
            UIManager.Instance.GetUICanvas<SettingsView>().SwitchGroup(GroupIndex);
        }

        public void SetRoot(Transform root, int groupIndex)
        {
            _content = root;
            GroupIndex = groupIndex;
        }

        public void LoadSettings(SettingGroup settingGroup, out Dictionary<string, Setting> created)
        {
            _title.SetText(settingGroup.GroupTitle);
            var settings = settingGroup.GetSettings(out created);
            foreach (var setting in settings)
            {
                _loadedSettings.Add(setting);
                setting.transform.SetParent(_content);
                setting.transform.localScale = Vector3.one;
            }
        }

        public void ShowGroup()
        {
            foreach (var setting in _loadedSettings)
            {
                setting.gameObject.SetActive(true);
            }
        }

        public void HideGroup()
        {
            foreach (var setting in _loadedSettings)
            {
                setting.gameObject.SetActive(false);
            }
        }
    }
}