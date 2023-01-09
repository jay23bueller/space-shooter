using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    #region Variables
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private SpawnManager _spawnManager;
    [SerializeField]
    private GameObject _explosionGO;
    #endregion

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.angularVelocity = -20f;
    }


    public void InitiateGame()
    {
        GetComponent<CircleCollider2D>().enabled = false;
        Destroy(Instantiate(_explosionGO, transform.position, Quaternion.identity), 2.2f);
        _spawnManager.StartWave(2f);
        Destroy(gameObject, .1f);
    }
    #endregion
}
