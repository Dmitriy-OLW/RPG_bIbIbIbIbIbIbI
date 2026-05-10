using UnityEngine;
using System;

namespace Score
{
    public class ScoreUnlocker : MonoBehaviour
    {
        [Serializable]
        public class ScoreUnlockEntry
        {
            public int requiredScore;
            public GameObject targetObject;
        }
        
        [SerializeField] private ScoreUnlockEntry[] _unlocks;
        [SerializeField] private bool _disableOnStart = true;
        
        private void OnEnable()
        {
            ScoreManager.OnScoreChanged += CheckScore;
        }
        
        private void OnDisable()
        {
            ScoreManager.OnScoreChanged -= CheckScore;
        }
        
        private void Start()
        {
            if (_disableOnStart)
            {
                foreach (var unlock in _unlocks)
                {
                    if (unlock.targetObject != null)
                        unlock.targetObject.SetActive(false);
                }
            }
            
            CheckScore(ScoreManager.CurrentScore);
        }
        
        private void CheckScore(int currentScore)
        {
            foreach (var unlock in _unlocks)
            {
                if (unlock.targetObject == null) continue;
                
                if (currentScore >= unlock.requiredScore)
                {
                    if (!unlock.targetObject.activeSelf)
                        unlock.targetObject.SetActive(true);
                }
            }
        }
    }
}