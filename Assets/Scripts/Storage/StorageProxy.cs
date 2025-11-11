using System;
using System.Collections.Generic;
using UnityEngine;

namespace Storage
{
    public static class StorageProxy
    {
        private const string SETTINGS_KEY = "Settings";
        private static Dictionary<string, object> _settings;
        private static bool _settingsLoaded;

        public static T GetSetting<T>(string key)
        {
            if (!_settingsLoaded) LoadSettings();
            return (T)_settings![key];
        }

        public static bool HasSetting(string key)
        {
            if (!_settingsLoaded) LoadSettings();
            return _settings.ContainsKey(key);
        }

        public static void SetSetting<T>(string key, T value)
        {
            if (!_settingsLoaded) LoadSettings();
            if (!_settings.TryAdd(key, value))
            {
                _settings[key] = value;
            }
        }

        private readonly static Dictionary<Type, Action<string, object>> Setters = new Dictionary<Type, Action<string, object>>
        {
            [typeof(float)] = (key, num) => PlayerPrefs.SetFloat(key, (float)num),
            [typeof(string)] = (key, str) => PlayerPrefs.SetString(key, (string)str),
            [typeof(int)] = (key, num) => PlayerPrefs.SetInt(key, (int)num)
        };
        private readonly static Dictionary<Type, Func<string, object>> Getters = new Dictionary<Type, Func<string, object>>
        {
            [typeof(float)] = key => PlayerPrefs.GetFloat(key),
            [typeof(string)] = PlayerPrefs.GetString,
            [typeof(int)] = key => PlayerPrefs.GetInt(key)
        };
        
        public static T Get<T>(string key)
        {
            if (PlayerPrefs.HasKey(key)) return (T)Getters[typeof(T)](key);

            Debug.LogError("PlayerPrefs does not contain key: " + key);
            return default;
        }

        public static void Set<T>(string key, T value)
        {
            Setters[typeof(T)](key, value);
            PlayerPrefs.Save();
        }
        
        public static bool HasKey(string key) => PlayerPrefs.HasKey(key);
        
        private static void LoadSettings()
        {
            var serialized = PlayerPrefs.GetString(SETTINGS_KEY);
            _settings = JsonUtility.FromJson<SerializableDictionary<string, object>>(serialized).ToDictionary();
            _settingsLoaded = true;
        }

        public static void SaveSettings()
        {
            var serializable = new SerializableDictionary<string, object>(_settings);
            var serialized = JsonUtility.ToJson(serializable);
            PlayerPrefs.SetString(SETTINGS_KEY, serialized);
        }
        
        public static void Delete(string key) => PlayerPrefs.DeleteKey(key);
        public static void Clear() => PlayerPrefs.DeleteAll();
    }
}