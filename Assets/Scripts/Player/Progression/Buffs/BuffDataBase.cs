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
        private bool _hasParsedBuffs;
        private List<PlayerBuff> _playerBuffs;
        private List<EnemyBuff> _enemyBuffs;

        public List<PlayerBuff> PlayerBuffs
        {
            get
            {
                if (!_hasParsedBuffs) ParseBuffs();
                
                return _playerBuffs;
            }
        }
        
        public List<EnemyBuff> EnemyBuffs
        {
            get
            {
                if (!_hasParsedBuffs) ParseBuffs();
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
            Debug.Log("Parsing Buffs");
            for (var i = 0; i < _buffs.Length; i++)
            {
                Debug.Log($"i => {_buffs[i] is PlayerBuff} / {_buffs[i] is EnemyBuff}");
                if (_buffs[i] is PlayerBuff) _playerBuffs.Add(Get<PlayerBuff>(i));
                else if (_buffs[i] is EnemyBuff) _enemyBuffs.Add(Get<EnemyBuff>(i));
            }

            _hasParsedBuffs = true;
        }

        public List<PlayerBuff> RandomPlayerBuff(int amount, int tier, int maxDeviation=1)
        {
            var players = PlayerBuffs;
            amount = Mathf.Min(amount, players.Count);
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
            amount = Mathf.Min(amount, players.Count);
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