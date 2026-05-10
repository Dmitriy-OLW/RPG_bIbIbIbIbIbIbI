using UnityEngine;
using System.Collections.Generic;

namespace SceneManagement
{
    public class SpawnerPositionRandomizer : MonoBehaviour
    {
        [SerializeField] private Transform[] _spawners;
        [SerializeField] private Transform[] _points;
        
        private void Start()
        {
            if (_spawners == null || _spawners.Length == 0)
                return;
            
            if (_points == null || _points.Length == 0)
                return;
            
            List<Transform> availablePoints = new List<Transform>(_points);
            
            foreach (Transform spawner in _spawners)
            {
                if (spawner == null) continue;
                
                if (availablePoints.Count == 0)
                    break;
                
                int randomIndex = Random.Range(0, availablePoints.Count);
                Transform selectedPoint = availablePoints[randomIndex];
                
                spawner.position = selectedPoint.position;
                availablePoints.RemoveAt(randomIndex);
            }
        }
    }
}