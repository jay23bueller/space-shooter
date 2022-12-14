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
    #endregion
    #region UnityMethods
    private void Start()
    {
        StartCoroutine(spawnEnemy());
    }
    #endregion

    #region Methods

    private IEnumerator spawnEnemy()
    {
        while (_canSpawn)
        {
            Vector3 spawnLocation = Camera.main.ViewportToWorldPoint(
                new Vector3(
                Random.Range(Enemy.LEFT_BOUND, Enemy.RIGHT_BOUND),
                Enemy.TOP_BOUND,
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

    public void Stop()
    {
        _canSpawn = false;
        Destroy(_enemyContainer);

    }
    #endregion
}
