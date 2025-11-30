using System.Collections.Generic;
using Core.Behaviour;
using Core.Cards.Hand;
using Player.Progression.Buffs;
using Player.Progression.Buffs.Enemy;
using Player.Progression.Buffs.Player;
using Storage;
using UI.View.MainMenuView;
using UnityEngine;

namespace Core.SessionStorage
{
    public class GameStorage : SingletonBase<GameStorage>
    {
        private const string PLAYER_BUFF_STORAGE_KEY = "Player Buffs";
        private const string ENEMY_BUFF_STORAGE_KEY = "Enemy Buffs";
        
        [SerializeField] private BuffDataBase _buffs;
        [SerializeField] private PlayerHand _playerHand;

        public int CurrentAct { get; private set; } = -1;
        public PlayerHand PlayerHand => _playerHand;
        public BuffStorage<PlayerBuff> PlayerBuffs { get; private set; } = new BuffStorage<PlayerBuff>();
        public BuffStorage<EnemyBuff> EnemyBuffs { get; private set; } = new BuffStorage<EnemyBuff>();

        protected override void Awake()
        {
            ToSingleton(true);
            Initialize();
        }

        protected override void Initialize()
        {
            if (StorageProxy.HasKey(PLAYER_BUFF_STORAGE_KEY))
                PlayerBuffs.Load(_buffs, StorageProxy.Get<string>(PLAYER_BUFF_STORAGE_KEY));
            if (StorageProxy.HasKey(ENEMY_BUFF_STORAGE_KEY))
                EnemyBuffs.Load(_buffs, StorageProxy.Get<string>(ENEMY_BUFF_STORAGE_KEY));
            
            var strings = StorageProxy.Get<string>(DeckView.DeckIDStorageKey).Split(',');
            var ids = new int[strings.Length];
            for (var i = 0; i < strings.Length; i++) ids[i] = int.Parse(strings[i]);
            _playerHand.Initialize(ids);
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
    }
}