using System;

namespace Pooling
{
    public interface IObjectPool<T> where T : IPoolable
    {
        T Get();
        void Return(T obj);
        void Prewarm(int count);
        void Clear();
    }
}