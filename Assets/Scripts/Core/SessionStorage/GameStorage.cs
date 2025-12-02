using System;
using System.Collections.Generic;
using Core.Behaviour;
using Core.Cards.Hand;
using Other.Dialog;
using Player.Progression.Buffs;
using Player.Progression.Buffs.Enemy;
using Player.Progression.Buffs.Player;
using Player.Progression.SaveStates;
using Storage;
using UI.View.MainMenuView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.SessionStorage
{
    public class GameStorage : SingletonBase<GameStorage>, IGameSerializable<SerializableGameStorage>
    {
        [SerializeField] private BuffDataBase _buffs;
        [SerializeField] private PlayerHand _playerHand;

        public int CurrentAct { get; private set; } = -1;
        public PlayerHand PlayerHand => _playerHand;
        public BuffDataBase BuffDataBase => _buffs;
        public BuffStorage<PlayerBuff> PlayerBuffs { get; } = new BuffStorage<PlayerBuff>();
        public BuffStorage<EnemyBuff> EnemyBuffs { get; } = new BuffStorage<EnemyBuff>();
        
        private readonly static Dictionary<string, Func<object>> SceneDataGetters = new Dictionary<string, Func<object>>
        {
            ["GameScene"] = () => GameManager.Instance.Board.SerializeSelf(),
            ["Dialogs"] = () => DialogSceneController.Instance.SerializeSelf()
        };
        
        private readonly static Dictionary<string, Action<object>> SceneDataSetters = new Dictionary<string, Action<object>>
        {
            ["GameScene"] = data => GameManager.Instance.Board.Deserialize((BoardState)data),
            ["Dialogs"] = data => DialogSceneController.Instance.Deserialize((DialogState)data)
        };

        protected override void Awake()
        {
            ToSingleton(true);
            Initialize();
        }

        protected override void Initialize()
        {
            if (GameSerializer.HasSavedData())
                Deserialize(GameSerializer.Deserialize().storage);
            else
            {
                var strings = StorageProxy.Get<string>(DeckView.DeckIDStorageKey).Split(',');
                var ids = new int[strings.Length];
                for (var i = 0; i < strings.Length; i++) ids[i] = int.Parse(strings[i]);
                _playerHand.Initialize(ids);
            }
        }

        public IList<BuffBase> GetRandomPlayerBuffOptions(int amount) =>
            _buffs.RandomPlayerBuff(amount, CurrentAct);

        public IList<BuffBase> GetRandomEnemyBuffOptions(int amount) =>
            _buffs.RandomEnemyBuff(amount, CurrentAct);

        public void AdvanceAct() => CurrentAct++;

        public void LoadEnemyBuffs(PlayerHand enemy)
        {
            foreach (var enemyBuff in EnemyBuffs.GetBuffs(ActivationType.Instant)) enemyBuff.Apply(enemy);
            foreach (var enemyBuff in EnemyBuffs.GetBuffs(ActivationType.ActStart)) enemyBuff.Apply(enemy);
        }

        public void AddBuff(BuffBase buff)
        {
            if (buff is PlayerBuff playerBuff)
            {
                if (playerBuff.Activation == ActivationType.Instant) playerBuff.Apply(_playerHand);
                else PlayerBuffs.Add(playerBuff);
            }
            else if (buff is EnemyBuff enemyBuff) EnemyBuffs.Add(enemyBuff);
        }

        private void OnApplicationPause(bool pauseStatus) => GameSerializer.Serialize();

        public SerializableGameStorage SerializeSelf()
        {
            var sceneData = SceneDataGetters[SceneManager.GetActiveScene().name];
            
            return new SerializableGameStorage(CurrentAct, _playerHand.SerializeSelf(),
                PlayerBuffs.Save(), EnemyBuffs.Save(), sceneData);
        }

        public void Deserialize(SerializableGameStorage self)
        {
            CurrentAct = self.Act;
            _playerHand.Deserialize(self.PlayerHand);
            PlayerBuffs.Load(_buffs, self.PlayerBuffs);
            EnemyBuffs.Load(_buffs, self.EnemyBuffs);
            SceneDataSetters[SceneManager.GetActiveScene().name](self.SceneData);
        }
    }

    public struct SerializableGameStorage
    {
        public int Act { get; }
        public SerializablePlayerHand PlayerHand { get; }
        public string PlayerBuffs { get; }
        public string EnemyBuffs { get; }
        public object SceneData { get; }

        public SerializableGameStorage(int act, SerializablePlayerHand playerHand, 
            string playerBuffs, string enemyBuffs, object sceneData)
        {
            Act = act;
            PlayerHand = playerHand;
            PlayerBuffs = playerBuffs;
            EnemyBuffs = enemyBuffs;
            SceneData = sceneData;
        }
    }
}