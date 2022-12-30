using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Powerup : MonoBehaviour
{
    #region Variables
    private float _speed = 3.0f;
    [SerializeField] //0 = TripleShot, 1 = SpeedBoost, 2 = Shield, 3 = Ammo
    private int _powerupID;
    [SerializeField]
    private AudioClip _powerupAudioClip;
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
                switch(_powerupID)
                {
                    case 0:
                        //Triple Shot
                        collision.GetComponent<Player>().EnableTripleShot();
                        break;
                    case 1:
                        //Speed Boost
                        collision.GetComponent<Player>().EnableSpeedBoost();
                        break;
                    case 2:
                        //Shields
                        collision.GetComponent<Player>().EnableShield();
                        break;
                    case 3:
                        //Ammo
                        collision.GetComponent<Player>().AddAmmo();
                        break;
                    default:
                        Debug.LogError("Didn't assign correct powerupID!");
                        break;
                }
                AudioSource.PlayClipAtPoint(_powerupAudioClip, transform.position, 3f);
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
