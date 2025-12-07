using Newtonsoft.Json;
using Other;
using Storage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.SessionStorage
{
    public static class GameSerializer
    {
        private const string STORAGE_SCENE_KEY = "Game Save Scene";
        private const string STORAGE_DATA_KEY = "Game Storage Save";
        
        public static void Serialize()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            StorageProxy.Set(STORAGE_DATA_KEY, JsonConvert.SerializeObject(GameStorage.Instance.SerializeSelf()));
            StorageProxy.Set(STORAGE_SCENE_KEY, sceneName);
        }

        public static (string scene, SerializableGameStorage storage) Deserialize()
        {
            var scene = StorageProxy.Get<string>(STORAGE_SCENE_KEY);
            var json = StorageProxy.Get<string>(STORAGE_DATA_KEY);
            var storageData = JsonConvert.DeserializeObject<SerializableGameStorage>(json);
            return (scene, storageData);
        }

        public static bool HasSavedData() => StorageProxy.HasKey(STORAGE_SCENE_KEY);

        public static void Clear()
        {
            StorageProxy.Delete(STORAGE_SCENE_KEY);
            StorageProxy.Delete(STORAGE_DATA_KEY);
        }
    }
}