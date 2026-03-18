using System.Collections.Generic;
using UnityEngine;
using Damage;

[CreateAssetMenu(fileName = "DamageConfig", menuName = "RPG/Damage Config")]
public class DamageDataSO : ScriptableObject
{
    public List<DamageData> DamageList;
}