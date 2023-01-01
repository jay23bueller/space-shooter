using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    #region Variables
    [SerializeField]
    protected float _speed = 8.0f;
    protected bool _canMove;
    protected bool _isEnemyWeapon;
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        CollisionCheck(ref collision);
    }
    #endregion

    #region Methods
    protected void CollisionCheck(ref Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("Player") && _isEnemyWeapon)
            {
                collision.GetComponent<Player>().UpdateLives(-1);
                Destroy(gameObject);
            }
        }
    }

    // Move laser till it's out of the viewport
    // at the moment, it is assumed to be moving to the top
    protected virtual void Move()
    {
        if (_canMove)
        {

            if (Camera.main.WorldToViewportPoint(transform.position).y < 1.0f && Camera.main.WorldToViewportPoint(transform.position).y > 0.0f)
                transform.Translate(transform.up * _speed * Time.deltaTime, Space.World);
            else
                Destroy(gameObject);
        }

    }

    public virtual void InitializeFiring(int owner)
    {
        switch(owner)
        {
            case 0:
                _isEnemyWeapon = true;
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
