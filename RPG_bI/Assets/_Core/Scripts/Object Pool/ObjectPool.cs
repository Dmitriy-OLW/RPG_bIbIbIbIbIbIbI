using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    public class ObjectPool<T> : IObjectPool<T> where T : MonoBehaviour, IPoolable
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly List<T> _activeObjects = new List<T>();
        private readonly int _maxSize;

        public ObjectPool(GameObject prefab, Transform parent, int initialSize = 10, int maxSize = 50)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;
            
            Prewarm(initialSize);
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj;
    
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                GameObject newObj = Object.Instantiate(_prefab, position, rotation, _parent);
                obj = newObj.GetComponentInChildren<T>();
                obj.OnReturnToPool += OnReturnToPool;
            }
            
            obj.transform.parent.SetPositionAndRotation(position, rotation);
    
            _activeObjects.Add(obj);
            return obj;
        }
        
        public T Get() => Get(Vector3.zero, Quaternion.identity);

        private void OnReturnToPool(IPoolable poolable)
        {
            if (poolable is T obj)
            {
                Return(obj);
            }
        }

        public void Return(T obj)
        {
            if (obj == null || !_activeObjects.Contains(obj))
                return;

            _activeObjects.Remove(obj);
            
            if (_pool.Count < _maxSize)
            {
                _pool.Enqueue(obj);
            }
            else
            {
                Object.Destroy(obj.transform.parent.gameObject);
            }
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject newObj = Object.Instantiate(_prefab, _parent);
                T obj = newObj.GetComponentInChildren<T>();
                obj.OnReturnToPool += OnReturnToPool;
                _pool.Enqueue(obj);
            }
        }

        public void Clear()
        {
            foreach (var obj in _activeObjects)
            {
                if (obj != null)
                    Object.Destroy(obj.transform.parent.gameObject);
            }
            
            foreach (var obj in _pool)
            {
                if (obj != null)
                    Object.Destroy(obj.transform.parent.gameObject);
            }
            
            _activeObjects.Clear();
            _pool.Clear();
        }
    }
}