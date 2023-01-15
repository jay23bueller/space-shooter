using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "enemyWaveInfo.asset", menuName = "SpawnInformation/EnemyWaveInfo")]
public class EnemyWaveInfo : ScriptableObject
{

    public EnemyInfo enemy;
    public float delayInbetween;
    public int numberOfEnemies;
}
