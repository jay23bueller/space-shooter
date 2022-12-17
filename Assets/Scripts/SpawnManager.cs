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
    private GameObject[] _powerups;
    public readonly static float LEFT_BOUND = 0.05f;
    public readonly static float RIGHT_BOUND = 0.95f;
    public readonly static float TOP_BOUND = 1.05f;
    public readonly static float BOTTOM_BOUND = -0.05f;
    #endregion
    #region UnityMethods
    private void Start()
    {
        StartCoroutine(SpawnEnemy());
        StartCoroutine(SpawnPowerup());
    }
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
                Quaternion.identity,
                _enemyContainer.transform
                );

            yield return new WaitForSeconds(Random.Range(_minSpawnTime, _maxSpawnTime + 1));
        }

    }

    private IEnumerator SpawnPowerup()
    {
        while(_canSpawn)
        {
            yield return new WaitForSeconds(Random.Range(3.0f, 7.0f));
            Vector3 spawnLocation = Camera.main.ViewportToWorldPoint(
                new Vector3(
                    Random.Range(LEFT_BOUND, RIGHT_BOUND),
                    TOP_BOUND,
                    Camera.main.WorldToViewportPoint(transform.position).z
                ));
            Instantiate(
                _powerups[Random.Range(0,_powerups.Length)],
                spawnLocation,
                Quaternion.identity
                );
        }
    }

    public void Stop()
    {
        _canSpawn = false;
        Destroy(_enemyContainer);

    }
    #endregion
}
