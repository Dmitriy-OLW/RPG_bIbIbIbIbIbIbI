using System.Collections.Generic;
using UnityEngine;
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
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform _spawnParent;
        
        private Dictionary<AIStateMachine, NPCTypes> _activeNPCTypes = new Dictionary<AIStateMachine, NPCTypes>();
        
        private Dictionary<NPCTypes, GameObject> _npcPrefabs;
        private Dictionary<AIStateMachine, NPCSpawnData> _activeNPCs;
        
        private void Awake()
        {
            _npcPrefabs = new Dictionary<NPCTypes, GameObject>();
            _activeNPCs = new Dictionary<AIStateMachine, NPCSpawnData>();
            
            if (_spawnParent == null)
            {
                _spawnParent = new GameObject("NPC_Spawns").transform;
                _spawnParent.SetParent(transform);
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
            if (!_npcPrefabs.ContainsKey(spawnData.npcType))
            {
                return;
            }
            
            Vector3 targetPosition = spawnData.spawnPoint != null 
                ? spawnData.spawnPoint.position 
                : _spawnParent.position;
            
            GameObject npcObject = Instantiate(_npcPrefabs[spawnData.npcType], targetPosition, Quaternion.identity, _spawnParent);
            AIStateMachine npc = npcObject.GetComponentInChildren<AIStateMachine>();
            
            if (npc == null)
            {
                Destroy(npcObject);
                return;
            }

            if (spawnData.patrolRouteParent != null)
            {
                List<Transform> patrolPoints = new List<Transform>();
                foreach (Transform child in spawnData.patrolRouteParent) 
                    patrolPoints.Add(child);
                npc.SetPatrolPoints(patrolPoints.ToArray());
            }

            if (npc.HealthController != null)
            {
                npc.HealthController.OnDeath += () => HandleNPCDeath(npc);
            }
    
            _activeNPCs[npc] = spawnData;
            _activeNPCTypes[npc] = spawnData.npcType; 
            
            npc.OnSpawn(targetPosition, healthOverride);
        }
        
        private void HandleNPCDeath(AIStateMachine npc)
        {
            if (npc == null) return;
            
            if (npc.HealthController != null)
            {
                npc.HealthController.OnDeath -= () => HandleNPCDeath(npc);
            }
            
            if (_activeNPCs.ContainsKey(npc))
            {
                _activeNPCs.Remove(npc);
            }
            
            if (_activeNPCTypes.ContainsKey(npc))
            {
                _activeNPCTypes.Remove(npc);
            } 
        }
        
        public void DespawnNPC(AIStateMachine npc)
        {
            if (npc == null) return;
            
            if (_activeNPCs.ContainsKey(npc))
            {
                if (npc.HealthController != null)
                {
                    npc.HealthController.OnDeath -= () => HandleNPCDeath(npc);
                }
                
                _activeNPCs.Remove(npc);
                _activeNPCTypes.Remove(npc);
                
                Destroy(npc.transform.parent != null ? npc.transform.parent.gameObject : npc.gameObject);
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
            _activeNPCs.Clear();
            _activeNPCTypes.Clear();
        }
    }
}