using System.Collections.Generic;
using System.Text;
using Core.Cards.Hand;
using Storage;

namespace Player.Progression.Buffs.Player
{
    public class PlayerBuffStorage
    {
        private const string PLAYER_BUFF_STORAGE_KEY = "Player Buffs";
        private PlayerHand _player;
        private Dictionary<ActivationType, List<PlayerBuff>> _buffs =
            new Dictionary<ActivationType, List<PlayerBuff>>();

        public List<PlayerBuff> GetBuffs(ActivationType ofType)
        {
            return _buffs.TryGetValue(ofType, out var buffs) ? buffs : new List<PlayerBuff>();
        }

        public void Add(PlayerBuff buff)
        {
            if (buff.Activation == ActivationType.Instant)
            {
                buff.Apply(_player);
                return;
            }

            if (!_buffs.ContainsKey(buff.Activation)) 
                _buffs.Add(buff.Activation, new List<PlayerBuff>());
            
            _buffs[buff.Activation].Add(buff);
        }

        public void Remove(PlayerBuff buff)
        {
            if (_buffs.ContainsKey(buff.Activation)) _buffs[buff.Activation].Remove(buff);
        }

        public void Clear()
        {
            _buffs.Clear();
            StorageProxy.Delete(PLAYER_BUFF_STORAGE_KEY);
        }
        
        public void Load(BuffDataBase db)
        {
            _buffs = new Dictionary<ActivationType, List<PlayerBuff>>();

            if (!StorageProxy.HasKey(PLAYER_BUFF_STORAGE_KEY)) return;

            var indexes = StorageProxy.Get<string>(PLAYER_BUFF_STORAGE_KEY).Split(',');
            foreach (var index in indexes)
            {
                var buff = db.Get<PlayerBuff>(int.Parse(index));
                Add(buff);
            }
        }

        public void Save()
        {
            var concat = new StringBuilder();
            foreach (var kvp in _buffs)
                foreach (var buff in kvp.Value)
                    concat.Append(buff.ID).Append(',');

            if (concat.Length > 0) concat.Remove(concat.Length - 1, 1);
            StorageProxy.Set(PLAYER_BUFF_STORAGE_KEY, concat.ToString());
        }
    }
}