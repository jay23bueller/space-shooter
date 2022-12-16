using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    #region Variables
    private float _speed = 3.0f;
    #endregion

    #region UnityMethods

    private void Update()
    {
        Move();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision != null)
        {
            if(collision.CompareTag("Player"))
            {
                collision.GetComponent<Player>().EnableTripleShot();
                Destroy(gameObject);
            }
        }
    }
    #endregion

    #region Methods

    private void Move()
    {
        if (Camera.main.WorldToViewportPoint(transform.position).y < -0.01f)
            Destroy(gameObject);
        else
            transform.Translate(Vector2.down * _speed * Time.deltaTime);
    }
    #endregion
}
