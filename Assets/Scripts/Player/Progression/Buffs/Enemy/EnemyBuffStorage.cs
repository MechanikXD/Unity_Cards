using System.Collections.Generic;
using System.Text;
using Core.Cards.Hand;
using Storage;

namespace Player.Progression.Buffs.Enemy
{
    public class EnemyBuffStorage
    {
        private const string ENEMY_BUFF_STORAGE_KEY = "Enemy Buffs";
        private Dictionary<ActivationType, List<EnemyBuff>> _buffs =
            new Dictionary<ActivationType, List<EnemyBuff>>();
        
        public List<EnemyBuff> GetBuffs(ActivationType ofType)
        {
            return _buffs.TryGetValue(ofType, out var buffs) ? buffs : new List<EnemyBuff>();
        }

        public void Add(EnemyBuff buff, PlayerHand enemy)
        {
            if (buff.Activation is ActivationType.Instant or ActivationType.ActStart)
            {
                buff.Apply(enemy);
                return;
            }

            if (!_buffs.ContainsKey(buff.Activation)) 
                _buffs.Add(buff.Activation, new List<EnemyBuff>());
            
            _buffs[buff.Activation].Add(buff);
        }

        public void AddWithoutApply(EnemyBuff buff)
        {
            if (!_buffs.ContainsKey(buff.Activation)) 
                _buffs.Add(buff.Activation, new List<EnemyBuff>());
            
            _buffs[buff.Activation].Add(buff);
        }

        public void ApplyAll(PlayerHand enemy, ActivationType ofType)
        {
            var buffs = GetBuffs(ofType);
            foreach (var buff in buffs) buff.Apply(enemy);
        }

        public void Remove(EnemyBuff buff)
        {
            if (_buffs.TryGetValue(buff.Activation, out var list)) list.Remove(buff);
        }

        public void Clear()
        {
            _buffs.Clear();
            StorageProxy.Delete(ENEMY_BUFF_STORAGE_KEY);
        }
        
        public void Load(BuffDataBase db, PlayerHand enemy)
        {
            _buffs = new Dictionary<ActivationType, List<EnemyBuff>>();

            if (!StorageProxy.HasKey(ENEMY_BUFF_STORAGE_KEY)) return;

            var indexes = StorageProxy.Get<string>(ENEMY_BUFF_STORAGE_KEY).Split(',');
            foreach (var index in indexes)
            {
                var buff = db.Get<EnemyBuff>(int.Parse(index));
                Add(buff, enemy);
            }
        }

        public void Save()
        {
            var concat = new StringBuilder();
            foreach (var kvp in _buffs)
                foreach (var buff in kvp.Value)
                    concat.Append(buff.ID).Append(',');

            if (concat.Length > 0) concat.Remove(concat.Length - 1, 1);
            StorageProxy.Set(ENEMY_BUFF_STORAGE_KEY, concat.ToString());
        }
    }
}