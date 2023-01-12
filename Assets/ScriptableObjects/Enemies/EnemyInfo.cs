using System;
using UnityEngine;



[CreateAssetMenu(fileName = "enemyInfo.asset",menuName = "SpawnInformation/EnemyInfo")]
public class EnemyInfo : ScriptableObject
{
    public MovementMode movementType;
    public GameObject enemyType;
    public Delays[] delaysPerWave = new Delays[3];
    [Serializable]
    public struct WeaponFireRateDelays
    {
        [SerializeField]
        private float _maxFireRateDelay;
        [SerializeField]
        private float _minFireRateDelay;

        public float maxFireRateDelay { get { return _maxFireRateDelay; } }
        public float minFireRateDelay { get { return _minFireRateDelay; } }

    }

    [Serializable]
    public struct SpawnDelays
    {
        [SerializeField]
        private float _maxSpawnDelay;
        [SerializeField]
        private float _minSpawnDelay;

        public float maxSpawnDelay { get { return _maxSpawnDelay; } }
        public float minSpawnDelay { get { return _minSpawnDelay; } }
    }

    [Serializable]
    public struct Delays
    {
        public SpawnDelays spawnDelays;
        public WeaponFireRateDelays weaponFireRateDelays;
    }

    
}
