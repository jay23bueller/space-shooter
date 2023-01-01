using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public enum Powerups
{
    TripleShot,
    HomingMissile,
    HealthCollectible,
    AmmoCollectible,
    Shield,
    SpeedBoost
}
public class Powerup : MonoBehaviour
{
    #region Variables
    private float _speed = 3.0f;
    [SerializeField] //0 = TripleShot, 1 = SpeedBoost, 2 = Shield, 3 = Ammo
    private Powerups _powerup;
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
                switch(_powerup)
                {
                    case Powerups.TripleShot:
                    case Powerups.HomingMissile:
                        FiringMode mode = _powerup == Powerups.TripleShot ? FiringMode.TripleShot : FiringMode.HomingMissile;
                        collision.GetComponent<Player>().EnableWeapon(mode);
                        break;
                    case Powerups.SpeedBoost:
                        collision.GetComponent<Player>().EnableSpeedBoost();
                        break;
                    case Powerups.Shield:
                        collision.GetComponent<Player>().EnableShield();
                        break;
                    case Powerups.AmmoCollectible:
                        collision.GetComponent<Player>().AddAmmo();
                        break;
                    case Powerups.HealthCollectible:
                        collision.GetComponent<Player>().UpdateLives(1);
                        break;
                    default:
                        Debug.LogError("Didn't assign correct powerupID!");
                        break;
                }
                AudioSource.PlayClipAtPoint(_powerupAudioClip, Camera.main.transform.position, .8f);
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
