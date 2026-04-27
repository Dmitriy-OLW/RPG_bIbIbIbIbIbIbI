using UnityEngine;
using Spawning;
using Spawning.Data;

namespace SceneManagement
{
    public class LocalWaveSpawner : MonoBehaviour
    {
        [SerializeField] private NPCSpawner _npcSpawner;
        [SerializeField] private NPCTypes[] _npcsToSpawn;
        [SerializeField] private Transform[] _patrolRoutes;
        [SerializeField] private float _spawnInterval = 20f;
        [SerializeField] private bool _loop = false;
        
        private int _currentIndex = 0;
        private float _spawnTimer = 0f;
        private bool _isSpawning = true;
        
        private void Start()
        {
            _spawnTimer = 0f;
        }
        
        private void FixedUpdate()
        {
            if (!_isSpawning) return;
            
            _spawnTimer += Time.deltaTime;
            
            if (_spawnTimer >= _spawnInterval)
            {
                SpawnNextNPC();
                _spawnTimer = 0f;
            }
        }
        
        private void SpawnNextNPC()
        {
            if (_currentIndex >= _npcsToSpawn.Length)
            {
                if (_loop)
                {
                    _currentIndex = 0;
                }
                else
                {
                    _isSpawning = false;
                    return;
                }
            }
            
            NPCTypes npcType = _npcsToSpawn[_currentIndex];
            Transform selectedRoute = GetRandomPatrolRoute();
            
            NPCSpawnData spawnData = new NPCSpawnData
            {
                npcType = npcType,
                spawnPoint = transform,
                patrolRouteParent = selectedRoute
            };
            
            _npcSpawner.SpawnNPC(spawnData);
            
            _currentIndex++;
        }
        
        private Transform GetRandomPatrolRoute()
        {
            if (_patrolRoutes == null || _patrolRoutes.Length == 0)
                return null;
            
            return _patrolRoutes[Random.Range(0, _patrolRoutes.Length)];
        }
        
        public void StartSpawning()
        {
            _isSpawning = true;
            _spawnTimer = 0f;
            _currentIndex = 0;
        }
        
        public void StopSpawning()
        {
            _isSpawning = false;
        }
    }
}