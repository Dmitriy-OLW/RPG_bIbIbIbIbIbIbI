using UnityEngine;
using System;

namespace Spawning.Data
{
    [Serializable]
    public class NPCSpawnData
    {
        public NPCTypes npcType;
        public Transform spawnPoint;
        public Transform patrolRouteParent;
    }
}