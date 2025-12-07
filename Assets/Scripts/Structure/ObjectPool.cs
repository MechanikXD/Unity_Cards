using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Structure
{
    public class ObjectPool<TObject> where TObject : MonoBehaviour
    {
        private readonly TObject _prefab;
        private int _maxLenght;
        private readonly Stack<TObject> _pool;
        
        private readonly Action<TObject> _onPull;
        private readonly Action<TObject> _onReturn;

        public ObjectPool(TObject prefab, int initialSize,
            [CanBeNull] Action<TObject> onPull, [CanBeNull] Action<TObject> onReturn)
        {
            _maxLenght = initialSize;
            _prefab = prefab;
            _pool = new Stack<TObject>(initialSize);
            _onPull = onPull ?? DefaultPull;
            _onReturn = onReturn ?? DefaultReturn;

            for (var i = 0; i < initialSize; i++)
            {
                var newObj = Object.Instantiate(prefab);
                _onReturn(newObj);
                _pool.Push(newObj);
            }
        }

        public TObject Pull()
        {
            if (_pool.Count == 0)
            {
                var newObj = Object.Instantiate(_prefab);
                _onPull(newObj);
                return newObj;
            }
            
            var obj = _pool.Pop();
            _onPull(obj);
            return obj;
        }

        public void Return(TObject returnee)
        {
            if (_pool.Count == _maxLenght) _maxLenght++;
            
            _onReturn(returnee);
            _pool.Push(returnee);
        }

        private static void DefaultReturn(TObject returnee) => returnee.gameObject.SetActive(false);
        private static void DefaultPull(TObject returnee) => returnee.gameObject.SetActive(true);
    }
}