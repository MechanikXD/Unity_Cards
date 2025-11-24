using System.Collections.Generic;
using Other.Extensions;
using Player.Progression.Buffs.Enemy;
using Player.Progression.Buffs.Player;
using UnityEngine;

namespace Player.Progression.Buffs
{
    [CreateAssetMenu(fileName = "Buff DataBase", menuName = "ScriptableObjects/Buff DataBase")]
    public class BuffDataBase : ScriptableObject
    {
        [SerializeField] private BuffBase[] _buffs;
        private List<PlayerBuff> _playerBuffs;
        private List<EnemyBuff> _enemyBuffs;

        public List<PlayerBuff> PlayerBuffs
        {
            get
            {
                if (_playerBuffs == null) ParseBuffs();
                return _playerBuffs;
            }
        }
        
        public List<EnemyBuff> EnemyBuffs
        {
            get
            {
                if (_enemyBuffs == null) ParseBuffs();
                return _enemyBuffs;
            }
        }

        public int Count => _buffs.Length;
        public T Get<T>(int index) where T : BuffBase
        {
            var buff = _buffs[index];
            buff.ID = index;
            return (T)buff;
        }

        private void ParseBuffs()
        {
            _playerBuffs = new List<PlayerBuff>();
            _enemyBuffs = new List<EnemyBuff>();
            for (var i = 0; i < _buffs.Length; i++)
            {
                if (_buffs[i] is PlayerBuff) _playerBuffs.Add(Get<PlayerBuff>(i));
                else if (_buffs[i] is EnemyBuff) _enemyBuffs.Add(Get<EnemyBuff>(i));
            }
        }

        public List<PlayerBuff> RandomPlayerBuff(int amount, int tier, int maxDeviation=1)
        {
            var players = PlayerBuffs;
            var indexes = players.ShuffledIndexes();
            var result = new List<PlayerBuff>();
            var counter = 0;
            foreach (var index in indexes)
            {
                if (Mathf.Abs(players[index].Tier - tier) < maxDeviation) continue;

                result.Add(players[index]);
                counter++;
                if (counter >= amount) break;
            }

            return result;
        }

        public List<EnemyBuff> RandomEnemyBuff(int amount, int tier, int maxDeviation=1)
        {
            var players = EnemyBuffs;
            var indexes = players.ShuffledIndexes();
            var result = new List<EnemyBuff>();
            var counter = 0;
            foreach (var index in indexes)
            {
                if (Mathf.Abs(players[index].Tier - tier) < maxDeviation) continue;

                result.Add(players[index]);
                counter++;
                if (counter >= amount) break;
            }

            return result;
        }
    }
}