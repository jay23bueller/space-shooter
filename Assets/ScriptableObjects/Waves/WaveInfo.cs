using System;
using UnityEngine;

[CreateAssetMenu(fileName = "waveInfo.asset", menuName ="SpawnInformation/WaveInfo")]
public class WaveInfo : ScriptableObject
{
    public WaveItem[] waveItems = new WaveItem[3];

    public bool isBossWave;

    [Serializable]
    public struct WaveItem
    {
        public EnemyWaveInfo enemyWaveInfo;
        public WaveItemMovementMode waveItemMovementMode;
    };

    public enum WaveItemMovementMode
    {
        normal,
        mirrored,
        random
    }

}
