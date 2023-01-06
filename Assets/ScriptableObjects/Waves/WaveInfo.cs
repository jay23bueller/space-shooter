using System;
using UnityEngine;

[CreateAssetMenu(fileName = "waveInfo.asset", menuName ="SpawnInformation/WaveInfo")]
public class WaveInfo : ScriptableObject
{
    public WaveItem[] waveItems = new WaveItem[3];


    [Serializable]
    public struct WaveItem
    {
        public EnemyInfo enemy;
        public bool mirroredMovement;
    };
}
