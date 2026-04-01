using System;
using System.Collections.Generic;
using UnityEngine;
using Spawning.Data;

namespace SaveSystem
{
    [Serializable]
    public class PlayerSaveData
    {
        public int playerId;
        public Vector3 position;
        public float currentHealth;
    }

    [Serializable]
    public class EnemySaveData
    {
        public NPCTypes npcType;
        public Vector3 position;
        public float currentHealth;
    }

    [Serializable]
    public class SceneSaveData
    {
        public string sceneName;
        public List<PlayerSaveData> players = new List<PlayerSaveData>();
        public List<EnemySaveData> enemies = new List<EnemySaveData>();
        public DateTime saveTime;
    }
}