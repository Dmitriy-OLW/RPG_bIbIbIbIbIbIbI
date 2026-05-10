using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Spawning;
using Spawning.Data;
using SaveSystem;

namespace SceneManagement
{
    public class SpawnerInitializer : MonoBehaviour
    {
        [SerializeField] private NPCSpawner _npcSpawner;
        [SerializeField] private NPCSpawnData[] _defaultSpawnData;

        public void InitializeDefault()
        {
            _npcSpawner.SpawnNPCs(_defaultSpawnData.ToList());
        }

        public void InitializeFromSave(List<EnemySaveData> savedEnemies)
        {
            var typeUsageCount = new Dictionary<NPCTypes, int>();

            foreach (var enemy in savedEnemies)
            {
                if (enemy.currentHealth <= 0) 
                    continue;
                
                if (!typeUsageCount.ContainsKey(enemy.npcType)) 
                    typeUsageCount[enemy.npcType] = 0;

                var route = GetPatrolRoute(enemy.npcType, typeUsageCount[enemy.npcType]);
                typeUsageCount[enemy.npcType]++;

                GameObject tempObj = new GameObject("TempPoint");
                tempObj.transform.position = enemy.position;

                var spawnData = new NPCSpawnData
                {
                    npcType = enemy.npcType,
                    spawnPoint = tempObj.transform,
                    patrolRouteParent = route
                };

                _npcSpawner.SpawnNPC(spawnData, enemy.currentHealth);
                Destroy(tempObj);
            }
        }

        private Transform GetPatrolRoute(NPCTypes type, int index)
        {
            var matches = _defaultSpawnData.Where(d => d.npcType == type).ToList();
            if (index < matches.Count) return matches[index].patrolRouteParent;
            
            return _defaultSpawnData[Random.Range(0, _defaultSpawnData.Length)].patrolRouteParent;
        }

        public List<EnemySaveData> GetActiveEnemiesData()
        {
            var data = new List<EnemySaveData>();
            foreach (var npc in _npcSpawner.ActiveNPCs)
            {
                var health = npc.Key.GetComponentInChildren<Health.HealthController>();
                
                if (health == null || health.CurrentHealth <= 0)
                    continue;
                
                data.Add(new EnemySaveData
                {
                    npcType = npc.Value,
                    position = npc.Key.transform.position,
                    currentHealth = health != null ? health.CurrentHealth : 0
                });
            }
            return data;
        }
    }
}