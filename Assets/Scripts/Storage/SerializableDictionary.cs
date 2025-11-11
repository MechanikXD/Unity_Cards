using System;
using System.Collections.Generic;

namespace Storage
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        public List<SerializableKeyValuePair<TKey, TValue>> List = new List<SerializableKeyValuePair<TKey, TValue>>();

        public SerializableDictionary(Dictionary<TKey, TValue> dictionary)
        {
            foreach (var pair in dictionary)
            {
                List.Add(new SerializableKeyValuePair<TKey, TValue>(pair.Key, pair.Value));
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var pair in List)
            {
                dictionary[pair.Key] = pair.Value;
            }
            return dictionary;
        }
    }
    
    [System.Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}