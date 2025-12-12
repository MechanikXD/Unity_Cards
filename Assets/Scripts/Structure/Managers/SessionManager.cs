using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cards.Hand;
using Dialogs;
using Enemy;
using Newtonsoft.Json.Linq;
using ProgressionBuffs;
using ProgressionBuffs.Enemy;
using ProgressionBuffs.Player;
using ProgressionBuffs.Scriptables;
using SaveLoad;
using SaveLoad.Serializeables;
using UI.View.MainMenuView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;

namespace Structure.Managers
{
    public class SessionManager : SingletonBase<SessionManager>, IGameSerializable<SerializableGameSession>
    {
        [SerializeField] private BuffDataBase _buffs;
        [SerializeField] private PlayerData _playerData;
        public EnemyDifficultySettings DifficultySettings { get; private set; }

        public bool HadLoadedData { get; set; }
        public int CurrentAct { get; private set; } = -1;
        public PlayerData PlayerData => _playerData;
        public BuffDataBase BuffDataBase => _buffs;
        public BuffStorage<PlayerBuff> PlayerBuffs { get; } = new BuffStorage<PlayerBuff>();
        public BuffStorage<EnemyBuff> EnemyBuffs { get; } = new BuffStorage<EnemyBuff>();

        private readonly Dictionary<string, float> _statistics = new Dictionary<string, float>();
        
        private readonly static Dictionary<string, Func<object>> SceneDataGetters = new Dictionary<string, Func<object>>
        {
            ["GameScene"] = () => GameManager.Instance.Board.SerializeSelf(),
            ["Dialogs"] = () => DialogSceneController.Instance.SerializeSelf()
        };
        
        private readonly static Dictionary<string, Action<JObject>> SceneDataSetters = new Dictionary<string, Action<JObject>>
        {
            ["GameScene"] = data => GameManager.Instance.Board.Deserialize(data.ToObject<BoardState>()),
            ["Dialogs"] = data => DialogSceneController.Instance.Deserialize(data.ToObject<DialogState>())
        };

        protected override void Awake()
        {
            ToSingleton(true);
            if (!WasMarkedToDestroy) Initialize();
        }

        protected override void Initialize()
        {
            if (GameSerializer.HasSavedData()) return;

            var strings = StorageProxy.Get<string>(DeckView.DeckIDStorageKey).Split(',');
            var ids = new int[strings.Length];
            for (var i = 0; i < strings.Length; i++) ids[i] = int.Parse(strings[i]);
            _playerData.Initialize(ids);
        }
        
        public void SetSettings(EnemyDifficultySettings settings) => 
            DifficultySettings = settings;
        
        public void SetSettings(string settings) =>
            DifficultySettings =
                Resources.LoadAll<EnemyDifficultySettings>("Settings/Enemy Settings")
                    .First(s => s.DifficultyName == settings);

        public IList<BuffBase> GetRandomPlayerBuffOptions(int amount) =>
            _buffs.RandomPlayerBuff(amount, CurrentAct);

        public IList<BuffBase> GetRandomEnemyBuffOptions(int amount) =>
            _buffs.RandomEnemyBuff(amount, CurrentAct);

        public void AdvanceAct()
        {
            CurrentAct++;
            AddStatistics(nameof(CurrentAct), CurrentAct);
        }

        public void AddStatistics(string key, float newValue)
        {
            if (!_statistics.TryAdd(key, newValue)) 
                _statistics[key] = newValue;
        }
        
        public void AddStatistics(string key, Func<float, float> modify, float defaultValue=0f)
        {
            if (!_statistics.TryAdd(key, modify(defaultValue))) 
                _statistics[key] = modify(_statistics[key]);
        }
        
        public void IncrementStatistics(string key)
        {
            if (!_statistics.TryAdd(key, 1f)) 
                _statistics[key]++;
        }

        public string GetFormatedStats()
        {
            var sb = new StringBuilder();
            foreach (var kvp in _statistics) sb.AppendLine($"{kvp.Key}: {kvp.Value}");

            return sb.ToString();
        }

        public void LoadEnemyBuffs(PlayerData enemy)
        {
            foreach (var enemyBuff in EnemyBuffs.GetBuffs(ActivationType.Instant)) enemyBuff.Apply(enemy);
            foreach (var enemyBuff in EnemyBuffs.GetBuffs(ActivationType.ActStart)) enemyBuff.Apply(enemy);
        }

        public bool PlayerHasBuff(int index) => PlayerBuffs.Contains(index);

        public void AddBuff(BuffBase buff)
        {
            if (buff is PlayerBuff playerBuff)
            {
                if (playerBuff.Activation == ActivationType.Instant) playerBuff.Apply(_playerData);
                PlayerBuffs.Add(playerBuff);
            }
            else if (buff is EnemyBuff enemyBuff) EnemyBuffs.Add(enemyBuff);
        }

        private void OnApplicationFocus(bool isFocus)
        {
            if (!isFocus) GameSerializer.Serialize();
        }

        public SerializableGameSession SerializeSelf()
        {
            var sceneData = SceneDataGetters[SceneManager.GetActiveScene().name];
            
            return new SerializableGameSession(CurrentAct, _playerData.SerializeSelf(),
                PlayerBuffs.Save(), EnemyBuffs.Save(), sceneData(), _statistics);
        }

        public void Deserialize(SerializableGameSession self)
        {
            CurrentAct = self._act;
            _playerData.Deserialize(self._playerHand);
            PlayerBuffs.Load(_buffs, self._playerBuffs);
            EnemyBuffs.Load(_buffs, self._enemyBuffs);
            SceneDataSetters[SceneManager.GetActiveScene().name](self.SceneData);
            foreach (var kvp in self.Statistics) AddStatistics(kvp.Key, kvp.Value);
            if (SceneManager.GetActiveScene().name == "GameScene") HadLoadedData = true;
        }
    }

    [Serializable]
    public class SerializableGameSession
    {
        public int _act;
        public SerializablePlayerHand _playerHand;
        public string _playerBuffs;
        public string _enemyBuffs;
        public JObject SceneData;
        public Dictionary<string, float> Statistics;

        public SerializableGameSession(int act, SerializablePlayerHand playerHand, 
            string playerBuffs, string enemyBuffs, object sceneData, Dictionary<string, float> statistics)
        {
            _act = act;
            _playerHand = playerHand;
            _playerBuffs = playerBuffs;
            _enemyBuffs = enemyBuffs;
            SceneData = JObject.FromObject(sceneData);
            Statistics = statistics;
        }
    }
}