using System;

namespace Pooling
{
    public interface IPoolable
    {
        event Action<IPoolable> OnReturnToPool;
        void OnSpawn();
        void OnDespawn();
    }
}