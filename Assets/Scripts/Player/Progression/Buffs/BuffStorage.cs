using System.Collections.Generic;
using System.Text;
using Core.Cards.Hand;

namespace Player.Progression.Buffs
{
    public class BuffStorage<T> where T : BuffBase
    {
        private Dictionary<ActivationType, List<T>> _buffs =
            new Dictionary<ActivationType, List<T>>();
        
        public List<T> GetBuffs(ActivationType ofType)
        {
            return _buffs.TryGetValue(ofType, out var buffs) ? buffs : new List<T>();
        }

        public void Add(T buff)
        {
            if (!_buffs.ContainsKey(buff.Activation)) 
                _buffs.Add(buff.Activation, new List<T>());
            
            _buffs[buff.Activation].Add(buff);
        }

        public void ApplyAll(PlayerHand player, ActivationType ofType)
        {
            var buffs = GetBuffs(ofType);
            foreach (var buff in buffs) buff.Apply(player);
        }

        public void Remove(T buff)
        {
            if (_buffs.TryGetValue(buff.Activation, out var list)) list.Remove(buff);
        }

        public void Clear() => _buffs.Clear();

        public void Load(BuffDataBase db, string buffIds)
        {
            _buffs = new Dictionary<ActivationType, List<T>>();

            var indexes = buffIds.Split(',');
            foreach (var index in indexes)
            {
                var buff = db.Get<T>(int.Parse(index));
                Add(buff);
            }
        }

        public string Save()
        {
            var concat = new StringBuilder();
            foreach (var kvp in _buffs)
                foreach (var buff in kvp.Value)
                    concat.Append(buff.ID).Append(',');

            if (concat.Length > 0) concat.Remove(concat.Length - 1, 1);
            return concat.ToString();
        }
    }
}