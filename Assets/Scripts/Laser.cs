using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private float _speed = 8.0f;
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
    #endregion

    #region Methods
    // Move laser till it's out of the viewport
    // at the moment, it is assumed to be moving to the top
    private void Move()
    {
        if (Camera.main.WorldToViewportPoint(transform.position).y < 1.0f)
            transform.Translate(transform.up * _speed * Time.deltaTime);
        else
            Destroy(gameObject);
    }
    #endregion
}
