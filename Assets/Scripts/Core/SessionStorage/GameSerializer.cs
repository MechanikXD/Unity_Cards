using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Other.Dialog;
using Storage;
using UnityEngine.SceneManagement;

namespace Core.SessionStorage
{
    public static class GameSerializer
    {
        private const string STORAGE_SCENE_KEY = "Game Save Scene";
        private const string STORAGE_KEY = "Game Save";

        private readonly static Dictionary<string, Func<object>> SerializeGetters = new Dictionary<string, Func<object>>
            {
                ["GameScene"] = () => GameManager.Instance.Board.SerializeSelf(),
                ["Dialogs"] = () => DialogSceneController.Instance.SerializeSelf()
            };
        
        public static void Serialize()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            StorageProxy.Set(STORAGE_KEY, JsonConvert.SerializeObject(SerializeGetters[sceneName]));
            StorageProxy.Set(STORAGE_SCENE_KEY, sceneName);
        }

        public static (string scene, T data) Deserialize<T>()
        {
            var scene = StorageProxy.Get<string>(STORAGE_SCENE_KEY);
            var json = StorageProxy.Get<string>(STORAGE_KEY);
            var obj = JsonConvert.DeserializeObject<T>(json);
            return (scene, obj);
        }

        public static bool HasSavedData() => StorageProxy.HasKey(STORAGE_SCENE_KEY);

        public static void Clear()
        {
            StorageProxy.Delete(STORAGE_KEY);
            StorageProxy.Delete(STORAGE_SCENE_KEY);
        }
    }
}