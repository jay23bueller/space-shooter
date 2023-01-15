using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    #region Variables
    private bool _canSpawn = true;
    


    [Header("Powerups")]
    [SerializeField]
    private GameObject[] _powerups;
    [SerializeField]
    private int _turnsBeforeSpawningAmmo = 4;
    [SerializeField]
    private int _enemySpawnsBeforeAmmoDrop = 4;
    [SerializeField]
    private AudioClip _healthDropClip;
    [SerializeField]
    private int _homingMissileSpawnInterval = 10;
    [SerializeField]
    private int _homingMissileIndex;
    [SerializeField]
    private int _healthCollectibleIndex;
    [SerializeField]
    private int _ammoCollectibleIndex;
    private int _powerupSpawnCount = 1;
    [SerializeField]
    private WeightedIndex[] _weightedIndices;

    [Header("Enemies")]
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private List<GameObject> _enemies = new List<GameObject>();



    [Header("Waves")]
    [SerializeField]
    private WaveInfo[] _waves;
    [SerializeField]
    private float _delayInBetweenWaves = 2.5f;
    private int _enemiesKilled;
    private int _currentWaveIndex;
    private int _currentWaveEnemyIndex;
    private bool _spawnedAllEnemiesInWave;

    [SerializeField]
    private UIManager _uiManager;
    private Player _player;

    #endregion
    #region UnityMethods
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    #endregion

    #region Methods

    private IEnumerator SpawnEnemy()
    {
        while (_currentWaveEnemyIndex < _waves[_currentWaveIndex].waveItems.Length)
        {
            Vector3 spawnLocation = Vector3.zero;
            WaveInfo.WaveItem waveItem = _waves[_currentWaveIndex].waveItems[_currentWaveEnemyIndex];

            //First enemy wave won't be delayed
            if (_currentWaveEnemyIndex != 0)
                yield return new WaitForSeconds(UnityEngine.Random.Range(waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].spawnDelays.minSpawnDelay, waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].spawnDelays.maxSpawnDelay));


            
                //Spawning number of enemies specified by the WaveInfo
                //Use the delayInBetween to delay spawning instances of current enemy
            for(int i = 0; i < waveItem.enemyWaveInfo.numberOfEnemies; i++)
            {
                bool isMirrored = false;
                switch (waveItem.waveItemMovementMode)
                {
                    case WaveInfo.WaveItemMovementMode.random:
                        isMirrored = UnityEngine.Random.value > 0.5f ? true : false;
                        break;
                    case WaveInfo.WaveItemMovementMode.mirrored:
                        isMirrored = true;
                        break;
                }
                switch (waveItem.enemyWaveInfo.enemy.movementType)
                {
                    case MovementMode.ZigZag:
                    case MovementMode.Vertical:
                    case MovementMode.WaypointDiamondPath:
                    case MovementMode.WaypointVPath:
                        spawnLocation =
                            new Vector3(
                                UnityEngine.Random.Range(GameManager.LEFT_BOUND, GameManager.RIGHT_BOUND),
                                GameManager.ENVIRONMENT_TOP_BOUND
                            );
                        break;
                    case MovementMode.Horizontal:
                        spawnLocation = new Vector3(
                            isMirrored ? GameManager.RIGHT_BOUND : GameManager.LEFT_BOUND,
                            UnityEngine.Random.Range(GameManager.ENVIRONMENT_TOP_BOUND * .5f, GameManager.ENVIRONMENT_TOP_BOUND * .8f)
                            );
                        break;
                    case MovementMode.Circular:
                        spawnLocation =
                            new Vector3(
                                isMirrored ? GameManager.RIGHT_BOUND : GameManager.LEFT_BOUND,
                                GameManager.ENVIRONMENT_TOP_BOUND
                                );
                        break;
                }


                if (_enemyContainer != null)
                {

                    GameObject enemy = Instantiate(
                    waveItem.enemyWaveInfo.enemy.enemyType,
                    spawnLocation,
                    Quaternion.AngleAxis(180, Vector3.forward),
                    _enemyContainer.transform);

                    if (enemy != null)
                    {
                        
                        enemy.GetComponent<Enemy>().SetMovementModeAndFiringDelays(waveItem.enemyWaveInfo.enemy.movementType, isMirrored, waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].weaponFireRateDelays.minFireRateDelay, waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].weaponFireRateDelays.maxFireRateDelay);
                        _enemies.Add(enemy);
                    }
                    



                }

                yield return new WaitForSeconds(waveItem.enemyWaveInfo.delayInbetween);
                    
            }

            _currentWaveEnemyIndex++;

            
        }
        _spawnedAllEnemiesInWave = true;
    }

    private IEnumerator SpawnPowerup()
    {
        while(_canSpawn)
        {
            int powerupIndex;
            if (_powerupSpawnCount % _turnsBeforeSpawningAmmo == 0)
                powerupIndex = _ammoCollectibleIndex;
            else
                powerupIndex = GetWeightedRandomIndex();


            yield return new WaitForSeconds(UnityEngine.Random.Range(3.0f, 7.0f));
            
            
            Vector3 spawnLocation = 
                new Vector3(
                    UnityEngine.Random.Range(GameManager.LEFT_BOUND, GameManager.RIGHT_BOUND), 
                    GameManager.ENVIRONMENT_TOP_BOUND
                    );

            Instantiate(
                _powerups[powerupIndex],
                spawnLocation,
                Quaternion.identity
                );
            _powerupSpawnCount++;
        }
    }

    private IEnumerator SpawnHomingMissile()
    {
        while (_canSpawn)
        {
            yield return new WaitForSeconds(_homingMissileSpawnInterval);

            Vector3 spawnLocation = new Vector3(UnityEngine.Random.Range(GameManager.LEFT_BOUND, GameManager.RIGHT_BOUND), GameManager.ENVIRONMENT_TOP_BOUND);
            Instantiate(
                _powerups[_homingMissileIndex],
                spawnLocation,
                Quaternion.identity
                );
            _powerupSpawnCount++;
        }
    }
    public void SpawnHealth()
    {
        Vector3 spawnLocation = 
            new Vector3(
                GameManager.RIGHT_BOUND - ((GameManager.RIGHT_BOUND - GameManager.LEFT_BOUND) * .5f),
                GameManager.ENVIRONMENT_TOP_BOUND
                );
            
        Instantiate(
            _powerups[_healthCollectibleIndex],
            spawnLocation,
            Quaternion.identity
            );

        AudioSource.PlayClipAtPoint(_healthDropClip, Camera.main.transform.position);
    }

    public void Stop()
    {
        StopAllCoroutines();
        _enemies.Clear();
        Destroy(_enemyContainer);

    }

    public void StartWave(float delay)
    {
        _uiManager.UpdateWaveText(_currentWaveIndex + 1);
        _uiManager.DisplayWaveText(true);
        StartCoroutine(StartWaveRoutine(delay));
    }

    public void EnemyDestroyed(GameObject enemy, float powerupSpawnDelayDuration, bool wasKilled)
    {
        if(_canSpawn)
        {
            _enemies.Remove(enemy);
            Vector3 position = enemy.transform.position;

            if (wasKilled)
                _enemiesKilled++;

            if(_enemiesKilled % _enemySpawnsBeforeAmmoDrop == 0)
            {
                StartCoroutine(SpawnPowerupAtEnemyPosition(position, powerupSpawnDelayDuration));
            }

            if (_spawnedAllEnemiesInWave && _enemies.Count == 0)
            {
                StopAllCoroutines();
                _currentWaveEnemyIndex = 0;
                _spawnedAllEnemiesInWave = false;
                _currentWaveIndex++;
                if (_currentWaveIndex < _waves.Length)
                {
                    StartWave(_delayInBetweenWaves);
                }
                else
                {
                    _uiManager.DisplayWinText();
                }
            }
        }
        
    }

    private IEnumerator SpawnPowerupAtEnemyPosition(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(
            _powerups[_ammoCollectibleIndex],
            position,
            Quaternion.identity
            );
    }

    private IEnumerator StartWaveRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _uiManager.DisplayWaveText(false);
        StartCoroutine(SpawnEnemy());
        StartCoroutine(SpawnPowerup());
        StartCoroutine(SpawnHomingMissile());
    }

    public Transform FindNearestEnemyToPlayer()
    {
        Transform closestEnemyTransform = null;
        
        float distance = -1f;
        foreach (GameObject enemy in _enemies)
        {
            if (_player == null)
                return null;

            if (distance == -1 || closestEnemyTransform == null)
            {
                closestEnemyTransform = enemy != null ? enemy.transform : null;

                if (closestEnemyTransform != null)
                    distance = Vector3.Distance(_player.transform.position, closestEnemyTransform.position);
            }
            else if (enemy != null)
            {
                float newEnemyDistance = Vector3.Distance(_player.transform.position, enemy.transform.position);
                if (newEnemyDistance < distance)
                {
                    closestEnemyTransform = enemy != null ? enemy.transform : closestEnemyTransform;
                    if (enemy != null)
                        distance = newEnemyDistance;
                }
            }
        }

        return closestEnemyTransform;

    }

    private int GetWeightedRandomIndex()
    {
        float totalWeight = _weightedIndices.Sum(currentWeightedIndex => currentWeightedIndex.weight);

        float scaledRandomValue = UnityEngine.Random.value * totalWeight;
        float currentTotal = 0f;

        for(int i = 0; i < _weightedIndices.Length; i++)
        {
            currentTotal += _weightedIndices[i].weight / totalWeight;

            if (currentTotal >= scaledRandomValue)
                return i;
        }

        return -1;
    }

    [Serializable]
    private struct WeightedIndex
    {
        public float weight;
        public int index;
    }
    #endregion
}
