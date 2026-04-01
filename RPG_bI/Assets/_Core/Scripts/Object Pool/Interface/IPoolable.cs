using System;
using UnityEngine;

namespace Pooling
{
    public interface IPoolable
    {
        event Action<IPoolable> OnReturnToPool;
        void OnSpawn(Vector3 spawnPosition, float healthOverride);
        void OnDespawn();
    }
}