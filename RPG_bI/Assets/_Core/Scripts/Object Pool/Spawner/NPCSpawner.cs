using System.Collections.Generic;
using UnityEngine;
using Pooling;
using Enemy.State;
using Spawning.Data;

namespace Spawning
{
    public class NPCSpawner : MonoBehaviour
    {
        [Header("NPC Library")]
        [SerializeField] private NPCLibrary _npcLibrary;
        
        [Header("Pool Settings")]
        [SerializeField] private Transform _poolParent;
        
        private Dictionary<NPCTypes, GameObject> _npcPrefabs;
        private Dictionary<NPCTypes, ObjectPool<AIStateMachine>> _pools;
        private Dictionary<AIStateMachine, NPCSpawnData> _activeNPCs;
        
        private void Awake()
        {
            _npcPrefabs = new Dictionary<NPCTypes, GameObject>();
            _pools = new Dictionary<NPCTypes, ObjectPool<AIStateMachine>>();
            _activeNPCs = new Dictionary<AIStateMachine, NPCSpawnData>();
            
            if (_poolParent == null)
            {
                _poolParent = new GameObject("NPC_Pool").transform;
                _poolParent.SetParent(transform);
            }
            
            InitializeLibrary();
        }
        
        private void InitializeLibrary()
        {
            if (_npcLibrary == null)
            {
                return;
            }
            
            foreach (var npcData in _npcLibrary.npcs)
            {
                if (npcData.prefab != null)
                {
                    _npcPrefabs[npcData.npcType] = npcData.prefab;
                }
            }
        }
        
        public void SpawnNPCs(List<NPCSpawnData> spawnDataList)
        {
            foreach (var spawnData in spawnDataList)
            {
                SpawnNPC(spawnData);
            }
        }
        
        public void SpawnNPC(NPCSpawnData spawnData)
        {
            if (!_npcPrefabs.ContainsKey(spawnData.npcType))
            {
                return;
            }
            
            GameObject prefab = _npcPrefabs[spawnData.npcType];
            
            if (!_pools.ContainsKey(spawnData.npcType))
            {
                var pool = new ObjectPool<AIStateMachine>(prefab, _poolParent, 1, 20);
                _pools[spawnData.npcType] = pool;
            }
            
            AIStateMachine npc = _pools[spawnData.npcType].Get();
            
            if (npc == null)
                return;
            
            if (spawnData.patrolRouteParent != null)
            {
                List<Transform> patrolPoints = new List<Transform>();
                foreach (Transform child in spawnData.patrolRouteParent)
                {
                    patrolPoints.Add(child);
                }
                npc.SetPatrolPoints(patrolPoints.ToArray());
            }
            
            GameObject npcRoot = npc.gameObject;
            if (spawnData.spawnPoint != null)
            {
                npcRoot.transform.position = spawnData.spawnPoint.position;
                npcRoot.transform.rotation = spawnData.spawnPoint.rotation;
            }

            _activeNPCs[npc] = spawnData;
            
            npc.OnSpawn();
        }
        
        public void DespawnNPC(AIStateMachine npc)
        {
            if (npc == null) return;
            
            if (_activeNPCs.ContainsKey(npc))
            {
                NPCTypes npcType = _activeNPCs[npc].npcType;
                if (_pools.ContainsKey(npcType))
                {
                    _pools[npcType].Return(npc);
                }
                _activeNPCs.Remove(npc);
            }
        }
        
        public void DespawnAllNPCs()
        {
            List<AIStateMachine> npcsToDespawn = new List<AIStateMachine>(_activeNPCs.Keys);
            
            foreach (var npc in npcsToDespawn)
            {
                DespawnNPC(npc);
            }
        }
        
        private void OnDestroy()
        {
            DespawnAllNPCs();
            
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            
            _pools.Clear();
            _activeNPCs.Clear();
        }
    }
}