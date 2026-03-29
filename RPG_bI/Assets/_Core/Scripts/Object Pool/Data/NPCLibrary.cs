using System.Collections.Generic;
using UnityEngine;

namespace Spawning.Data
{
    [CreateAssetMenu(fileName = "NPCLibrary", menuName = "RPG/NPC Library")]
    public class NPCLibrary : ScriptableObject
    {
        public NPCData[] npcs;
    }
}