using System;
using System.Collections.Generic;
using UI.Settings.Types;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.Settings
{
    [CreateAssetMenu(fileName = "Settings Data", menuName = "ScriptableObjects/Settings Data")]
    public class SettingsData : ScriptableObject
    {
        [SerializeField] private SettingGroup[] _settingGroups;
        
        public SettingGroup[] SettingGroups =>  _settingGroups;
    }

    [Serializable] public struct SettingGroup
    {
        [SerializeField] private string _groupTitle;
        
        [SerializeField] private SliderSettingValues[] _sliders;
        [SerializeField] private BooleanSettingValues[] _booleans;
        [SerializeField] private DropDownSettingValues[] _dropdowns;
        
        public string GroupTitle => _groupTitle;

        public List<Setting> GetSettings(out Dictionary<string, Setting> created)
        {
            var sliderPrefab = Resources.Load<SliderSetting>("Prefabs/Settings/Settings Slider");
            var booleanPrefab = Resources.Load<BooleanSetting>("Prefabs/Settings/Settings Boolean");
            var dropdownPrefab = Resources.Load<DropDownSetting>("Prefabs/Settings/Settings DropDown");
            
            created = new Dictionary<string, Setting>();
            var ordered = new List<Setting>();
            var currentOrder = 0;
            
            var sliderMaxOrder = _sliders.Length > 0 ? _sliders[^1].Order : -1;
            var boolMaxOrder = _booleans.Length > 0 ? _booleans[^1].Order : -1;
            var dropMaxOrder = _dropdowns.Length > 0 ? _dropdowns[^1].Order : -1;
            var maxOrder = Mathf.Max(sliderMaxOrder, boolMaxOrder, dropMaxOrder) + 1;
            
            var slidersLocalIndex = 0;
            var booleansLocalIndex = 0;
            var dropDownLocalIndex = 0;
            while (currentOrder < maxOrder)
            {
                while (slidersLocalIndex < _sliders.Length)
                {
                    if (_sliders[slidersLocalIndex].Order != currentOrder) break;
                    var newSetting = _sliders[slidersLocalIndex].GetSetting(sliderPrefab);
                    ordered.Add(newSetting);
                    created.Add(_sliders[slidersLocalIndex].SettingName, newSetting);
                    slidersLocalIndex++;
                }
                while (booleansLocalIndex < _booleans.Length)
                {
                    if (_booleans[booleansLocalIndex].Order != currentOrder) break;
                    var newSetting = _booleans[booleansLocalIndex].GetSetting(booleanPrefab);
                    ordered.Add(newSetting);
                    created.Add(_booleans[booleansLocalIndex].SettingName, newSetting);
                    booleansLocalIndex++;
                }
                while (dropDownLocalIndex < _dropdowns.Length)
                {
                    if (_dropdowns[dropDownLocalIndex].Order != currentOrder) break;
                    var newSetting = _dropdowns[dropDownLocalIndex].GetSetting(dropdownPrefab);
                    ordered.Add(newSetting);
                    created.Add(_dropdowns[dropDownLocalIndex].SettingName, newSetting);
                    dropDownLocalIndex++;
                }
                
                currentOrder++;
            }
            
            return ordered;
        }
    }
    
    [Serializable] public struct SliderSettingValues
    {
        [SerializeField] private string _settingName;
        [SerializeField] private int _order;
        [SerializeField] private float _defaultValue;
        [SerializeField] private Vector2 _bounds;
        [SerializeField] private bool _wholeNumbers;
        
        public string SettingName => _settingName;
        public int Order => _order;

        public SliderSetting GetSetting(SliderSetting prefab)
        {
            var slider = Object.Instantiate(prefab);
            slider.Load(_settingName, _defaultValue, _bounds, _wholeNumbers);
            return slider;
        }
    }
        
    [Serializable] public struct BooleanSettingValues
    {
        [SerializeField] private string _settingName;
        [SerializeField] private int _order;
        [SerializeField] private bool _isOnByDefault;
        
        public string SettingName => _settingName;
        public int Order => _order;
        
        public BooleanSetting GetSetting(BooleanSetting prefab)
        {
            var boolean = Object.Instantiate(prefab);
            boolean.Load(_settingName, _isOnByDefault);
            return boolean;
        }
    }
        
    [Serializable] public struct DropDownSettingValues
    {
        [SerializeField] private string _settingName;
        [SerializeField] private int _order;
        [SerializeField] private string[] _values;
        
        public string SettingName => _settingName;
        public int Order => _order;
        
        public DropDownSetting GetSetting(DropDownSetting prefab)
        {
            var dropDown = Object.Instantiate(prefab);
            dropDown.Load(_settingName, _values);
            return dropDown;
        }
    }
}