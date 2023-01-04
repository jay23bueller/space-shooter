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
    private List<GameObject> _enemies = new List<GameObject>();
    [SerializeField]
    private Player _player;
    [SerializeField]
    private GameObject[] _powerups;
    private int _powerupSpawnCount = 1;
    private int _enemiesDestroyedCount;
    [SerializeField]
    private int _turnsBeforeSpawningAmmo = 4;
    [SerializeField]
    private int _enemySpawnsBeforeAmmoDrop = 4;
    [SerializeField]
    private AudioClip _healthDropClip;
    [SerializeField]
    private int _homingMissileSpawnInterval = 10;
    #endregion
    #region UnityMethods

    #endregion

    #region Methods

    private IEnumerator SpawnEnemy()
    {
        while (_canSpawn)
        {
            Vector3 spawnLocation = 
                new Vector3(
                Random.Range(GameManager.LEFT_BOUND,GameManager.RIGHT_BOUND),
                GameManager.ENVIRONMENT_TOP_BOUND
               );

             _enemies.Add(Instantiate(
                _enemyPrefab,
                spawnLocation,
                Quaternion.AngleAxis(180, Vector3.forward),
                _enemyContainer.transform));


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
                powerupIndex = Random.Range(0, 2);


            yield return new WaitForSeconds(Random.Range(3.0f, 7.0f));
            
            
            Vector3 spawnLocation = 
                new Vector3(
                    Random.Range(GameManager.LEFT_BOUND, GameManager.RIGHT_BOUND), 
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

            Vector3 spawnLocation = new Vector3(Random.Range(GameManager.LEFT_BOUND, GameManager.RIGHT_BOUND), GameManager.ENVIRONMENT_TOP_BOUND);
            Instantiate(
                _powerups[5],
                spawnLocation,
                Quaternion.identity
                );
            _powerupSpawnCount++;
        }
    }
    public void SpawnHealth()
    {
        Vector3 spawnLocation = Camera.main.ViewportToWorldPoint(
            new Vector3(
                .5f,
                GameManager.ENVIRONMENT_TOP_BOUND,
                Camera.main.WorldToViewportPoint(transform.position).z
                ));
            
        Instantiate(
            _powerups[4],
            spawnLocation,
            Quaternion.identity
            );

        AudioSource.PlayClipAtPoint(_healthDropClip, Camera.main.transform.position);
    }

    public void Stop()
    {
        _canSpawn = false;
        _enemies.Clear();
        Destroy(_enemyContainer);

    }

    public void StartWave()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        StartCoroutine(StartWaveRoutine());
    }

    public void EnemyDestroyed(GameObject enemy, float powerupSpawnDelayDuration)
    {
        if(_canSpawn)
        {
            _enemies.Remove(enemy);
            Vector3 position = enemy.transform.position;
            _enemiesDestroyedCount++;
            if(_enemiesDestroyedCount % _enemySpawnsBeforeAmmoDrop == 0)
            {
                StartCoroutine(SpawnPowerupAtEnemyPosition(position, powerupSpawnDelayDuration));
            }
        }
        
    }

    private IEnumerator SpawnPowerupAtEnemyPosition(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(
            _powerups[3],
            position,
            Quaternion.identity
            );
    }

    private IEnumerator StartWaveRoutine()
    {
        yield return new WaitForSeconds(3f);
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
    #endregion
}
