// SpawnerInitializer.cs
using System.Collections.Generic;
using UnityEngine;
using Spawning;
using Spawning.Data;

namespace SceneManagement
{
    public class SpawnerInitializer : MonoBehaviour
    {
        [SerializeField] private NPCSpawner _npcSpawner;
        [SerializeField] private NPCSpawnData[] _spawnDataArray;

        [SerializeField] private bool Active = false;
        
        private void Start()
        {
            if (_npcSpawner == null)
            {
                _npcSpawner = FindObjectOfType<NPCSpawner>();
            }
            
            List<NPCSpawnData> spawnDataList = new List<NPCSpawnData>(_spawnDataArray);
            _npcSpawner.SpawnNPCs(spawnDataList);
        }
        
        private void Update()
        {
            if (Active)
            {
                List<NPCSpawnData> spawnDataList = new List<NPCSpawnData>(_spawnDataArray);
                _npcSpawner.SpawnNPCs(spawnDataList);
                Active = false;
            }
        }
    }
}