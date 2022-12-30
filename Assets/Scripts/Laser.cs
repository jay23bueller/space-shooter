using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private float _speed = 8.0f;
    private bool _canMove;
    private bool _isEnemyLaser;
    #endregion

    #region UnityMethods
    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void OnDestroy()
    {
        //TripleShot
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision != null)
        {
            if(collision.CompareTag("Player") && _isEnemyLaser)
            {
                collision.GetComponent<Player>().UpdateLives(-1);
                Destroy(gameObject);
            }
        }
    }
    #endregion

    #region Methods
    // Move laser till it's out of the viewport
    // at the moment, it is assumed to be moving to the top
    private void Move()
    {
        if (_canMove)
        {

            if (Camera.main.WorldToViewportPoint(transform.position).y < 1.0f && Camera.main.WorldToViewportPoint(transform.position).y > 0.0f)
                transform.Translate(transform.up * _speed * Time.deltaTime, Space.World);
            else
                Destroy(gameObject);
        }

    }

    public void InitializeFiring(int owner)
    {
        switch(owner)
        {
            case 0:
                _isEnemyLaser = true;
                tag = "EnemyLaser";
                break;
            case 1:
                tag = "Laser";
                break;
            default:
                Debug.LogError("Incorrect value for InitializingFire!");
                break;
        }

        _canMove = true;
    }
    #endregion
}
