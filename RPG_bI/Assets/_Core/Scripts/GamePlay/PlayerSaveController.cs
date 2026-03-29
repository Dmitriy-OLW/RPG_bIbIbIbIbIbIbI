using System.Collections.Generic;
using UnityEngine;
using Health;
using SaveSystem;

namespace SceneManagement
{
    public class PlayerSaveController : MonoBehaviour
    {
        [SerializeField] private List<HealthController> _playerHealths;

        public List<PlayerSaveData> GetPlayersData()
        {
            var data = new List<PlayerSaveData>();
            for (int i = 0; i < _playerHealths.Count; i++)
            {
                data.Add(new PlayerSaveData
                {
                    playerId = i,
                    position = _playerHealths[i].transform.position,
                    currentHealth = _playerHealths[i].CurrentHealth
                });
            }
            return data;
        }

        public void ApplyPlayerData(List<PlayerSaveData> data)
        {
            for (int i = 0; i < data.Count && i < _playerHealths.Count; i++)
            {
                _playerHealths[i].transform.position = data[i].position;
                if (data[i].currentHealth == 0)
                    _playerHealths[i].ResetHealth(10f);
                else
                    _playerHealths[i].ResetHealth(data[i].currentHealth);
            }
        }

        public void ApplyDefaultHealth()
        {
            for (int i = 0; i < _playerHealths.Count; i++)
            {
                _playerHealths[i].ResetHealth();
            }
        }
    }
}