using System.Collections.Generic;
using UnityEngine;
using Pooling;
using Enemy.State;
using Spawning.Data;
using Health;

namespace Spawning
{
    public class NPCSpawner : MonoBehaviour
    {
        public IReadOnlyDictionary<AIStateMachine, NPCTypes> ActiveNPCs => _activeNPCTypes;
        
        [Header("NPC Library")]
        [SerializeField] private NPCLibrary _npcLibrary;
        
        [Header("Pool Settings")]
        [SerializeField] private Transform _poolParent;
        
        private Dictionary<AIStateMachine, NPCTypes> _activeNPCTypes = new Dictionary<AIStateMachine, NPCTypes>();
        
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
            SpawnNPC(spawnData, -1f);
        }

        public void SpawnNPC(NPCSpawnData spawnData, float healthOverride = -1f)
        {
            if (!_npcPrefabs.ContainsKey(spawnData.npcType)) return;

            if (!_pools.ContainsKey(spawnData.npcType))
            {
                _pools[spawnData.npcType] = new ObjectPool<AIStateMachine>(_npcPrefabs[spawnData.npcType], _poolParent, 1, 20);
            }
            
            Vector3 targetPosition = spawnData.spawnPoint != null 
                ? spawnData.spawnPoint.position 
                : _poolParent.position;
            
            AIStateMachine npc = _pools[spawnData.npcType].Get(targetPosition, Quaternion.identity);
            if (npc == null) return;

            if (spawnData.patrolRouteParent != null)
            {
                List<Transform> patrolPoints = new List<Transform>();
                foreach (Transform child in spawnData.patrolRouteParent) patrolPoints.Add(child);
                npc.SetPatrolPoints(patrolPoints.ToArray());
            }
    
            _activeNPCs[npc] = spawnData;
            _activeNPCTypes[npc] = spawnData.npcType; 
            
            npc.OnSpawn(targetPosition, healthOverride);
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