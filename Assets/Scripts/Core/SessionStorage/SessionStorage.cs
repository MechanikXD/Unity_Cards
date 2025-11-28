using Core.Behaviour;
using Core.Cards.Hand;
using Player.Progression.Buffs;
using Player.Progression.Buffs.Enemy;
using Player.Progression.Buffs.Player;
using Storage;
using UnityEngine;

namespace Core.SessionStorage
{
    public class SessionStorage : SingletonBase<SessionStorage>
    {
        private const string PLAYER_BUFF_STORAGE_KEY = "Player Buffs";
        private const string ENEMY_BUFF_STORAGE_KEY = "Enemy Buffs";
        
        [SerializeField] private BuffDataBase _buffs;
        [SerializeField] private PlayerHand _player;
        [SerializeField] private PlayerHand _enemy;
        
        public int CurrentAct { get; private set; }

        public BuffStorage<PlayerBuff> PlayerBuffs { get; private set; } = new BuffStorage<PlayerBuff>();
        public BuffStorage<EnemyBuff> EnemyBuffs { get; private set; } = new BuffStorage<EnemyBuff>();

        public void AddBuffToPlayer(PlayerBuff buff)
        {
            if (buff.Activation == ActivationType.Instant)
            {
                buff.Apply(_player);
                return;
            }
            
            PlayerBuffs.Add(buff);
        }

        public void AddBuffToEnemy(EnemyBuff buff) => EnemyBuffs.Add(buff);

        public void LoadEnemyBuffs()
        {
            foreach (var enemyBuff in EnemyBuffs.GetBuffs(ActivationType.Instant)) enemyBuff.Apply(_enemy);
            foreach (var enemyBuff in EnemyBuffs.GetBuffs(ActivationType.ActStart)) enemyBuff.Apply(_enemy);
        }

        public void AddBuff(BuffBase buff)
        {
            if (buff is PlayerBuff playerBuff) AddBuffToPlayer(playerBuff);
            else if (buff is EnemyBuff enemyBuff) AddBuffToEnemy(enemyBuff);
        }

        protected override void Initialize()
        {
            PlayerBuffs.Load(_buffs, StorageProxy.Get<string>(PLAYER_BUFF_STORAGE_KEY));
            EnemyBuffs.Load(_buffs, StorageProxy.Get<string>(ENEMY_BUFF_STORAGE_KEY));
        }
    }
}