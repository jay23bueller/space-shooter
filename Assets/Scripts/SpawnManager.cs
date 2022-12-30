using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    #region Variables
    private bool _canSpawn = true;
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private int _minSpawnTime = 2;
    [SerializeField]
    private int _maxSpawnTime = 5;
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private GameObject[] _powerups;
    private int _powerupSpawnCount = 1;
    private int _enemiesDestroyedCount;
    [SerializeField]
    private int _turnsBeforeSpawningAmmo = 4;
    [SerializeField]
    private int _enemySpawnsBeforeAmmoDrop = 4;
    public readonly static float LEFT_BOUND = 0.05f;
    public readonly static float RIGHT_BOUND = 0.95f;
    public readonly static float TOP_BOUND = 1.05f;
    public readonly static float BOTTOM_BOUND = -0.05f;
    #endregion
    #region UnityMethods

    #endregion

    #region Methods

    private IEnumerator SpawnEnemy()
    {
        while (_canSpawn)
        {
            Vector3 spawnLocation = Camera.main.ViewportToWorldPoint(
                new Vector3(
                Random.Range(LEFT_BOUND,RIGHT_BOUND),
                TOP_BOUND,
                Camera.main.WorldToViewportPoint(_enemyContainer.transform.position).z
                ));

            GameObject newEnemy = Instantiate(
                _enemyPrefab,
                spawnLocation,
                Quaternion.AngleAxis(180, Vector3.forward),
                _enemyContainer.transform);

            yield return new WaitForSeconds(Random.Range(_minSpawnTime, _maxSpawnTime + 1));
        }

    }

    private IEnumerator SpawnPowerup()
    {
        while(_canSpawn)
        {
            int powerupIndex;
            if (_powerupSpawnCount % _turnsBeforeSpawningAmmo == 0)
                powerupIndex = 3;
            else
                powerupIndex = Random.Range(0, _powerups.Length-1);


            yield return new WaitForSeconds(Random.Range(3.0f, 7.0f));
            
            
            Vector3 spawnLocation = Camera.main.ViewportToWorldPoint(
                new Vector3(
                    Random.Range(LEFT_BOUND, RIGHT_BOUND),
                    TOP_BOUND,
                    Camera.main.WorldToViewportPoint(transform.position).z
                ));
            Instantiate(
                _powerups[powerupIndex],
                spawnLocation,
                Quaternion.identity
                );
            _powerupSpawnCount++;
        }
    }

    public void Stop()
    {
        _canSpawn = false;
        Destroy(_enemyContainer);

    }

    public void StartWave()
    {
        StartCoroutine(StartWaveRoutine());
    }

    public void EnemyDestroyed(Vector2 position)
    {
        if(_canSpawn)
        {
            _enemiesDestroyedCount++;
            if(_enemiesDestroyedCount % _enemySpawnsBeforeAmmoDrop == 0)
            {
                Instantiate(
                    _powerups[3],
                    position,
                    Quaternion.identity
                    );
            }
        }
        
    }

    private IEnumerator StartWaveRoutine()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(SpawnEnemy());
        StartCoroutine(SpawnPowerup());
    }
    #endregion
}
