using System;
using System.Collections;
using System.Collections.Generic;
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
    private int _numOfMovementModes;
    #endregion
    #region UnityMethods
    private void Start()
    {
        _numOfMovementModes = Enum.GetNames(typeof(MovementMode)).Length;
    }
    #endregion

    #region Methods

    private IEnumerator SpawnEnemy()
    {
        while (_canSpawn)
        {

            MovementMode mode = (MovementMode)UnityEngine.Random.Range(0, _numOfMovementModes);
            bool isMirrored = false;
            Vector3 spawnLocation = Vector3.zero;

            if(mode == MovementMode.Circular || mode == MovementMode.ZigZag)
            {
                isMirrored = UnityEngine.Random.Range(-1, 2) < 0 ? true : false;
            }

            switch (mode)
            {
                case MovementMode.ZigZag:
                case MovementMode.Vertical:
                    spawnLocation =
                        new Vector3(
                            UnityEngine.Random.Range(GameManager.LEFT_BOUND, GameManager.RIGHT_BOUND),
                            GameManager.ENVIRONMENT_TOP_BOUND
                        );
                    break;
                case MovementMode.Horizontal:
                    spawnLocation = new Vector3(
                        GameManager.LEFT_BOUND,
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


            GameObject enemy = Instantiate(
                _enemyPrefab,
                spawnLocation,
                Quaternion.AngleAxis(180, Vector3.forward),
                _enemyContainer.transform);

            if (enemy != null)
            {
                enemy.GetComponent<Enemy>().SetMovementMode(mode, isMirrored);
                _enemies.Add(enemy);
            }




            yield return new WaitForSeconds(UnityEngine.Random.Range(_minSpawnTime, _maxSpawnTime + 1));
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
                powerupIndex = UnityEngine.Random.Range(0, 3);


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
                _powerups[5],
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
                GameManager.RIGHT_BOUND * .5f,
                GameManager.ENVIRONMENT_TOP_BOUND
                );
            
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
