using Newtonsoft.Json;
using Structure;
using Structure.Managers;
using UnityEngine.SceneManagement;

namespace SaveLoad
{
    public static class GameSerializer
    {
        private const string STORAGE_SCENE_KEY = "Game Save Scene";
        private const string STORAGE_DATA_KEY = "Game Storage Save";
        
        public static void Serialize()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            StorageProxy.Set(STORAGE_DATA_KEY, JsonConvert.SerializeObject(SessionManager.Instance.SerializeSelf()));
            StorageProxy.Set(STORAGE_SCENE_KEY, sceneName);
        }

        public static (string scene, SerializableGameSession storage) Deserialize()
        {
            var scene = StorageProxy.Get<string>(STORAGE_SCENE_KEY);
            var json = StorageProxy.Get<string>(STORAGE_DATA_KEY);
            var storageData = JsonConvert.DeserializeObject<SerializableGameSession>(json);
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