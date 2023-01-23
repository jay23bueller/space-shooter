using UnityEngine;

public enum PowerupType
{
    TripleShot,
    HomingMissile,
    HealthCollectible,
    AmmoCollectible,
    Shield,
    SpeedBoost,
    WeaponDisruption,
    EnergyCollectible
}
public class Powerup : MonoBehaviour
{
    #region Variables
    private float _speed = 3.0f;
    [SerializeField] //0 = TripleShot, 1 = SpeedBoost, 2 = Shield, 3 = Ammo
    private PowerupType _powerup;
    [SerializeField]
    private AudioClip _powerupAudioClip;
    [SerializeField]
    private GameObject _explosionPrefab;
    private bool _beingDestroyed;
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
            if(collision.CompareTag("Player") && !_beingDestroyed)
            {
                _beingDestroyed = true;
                switch (_powerup)
                {
                    case PowerupType.TripleShot:
                    case PowerupType.HomingMissile:
                        FiringMode mode = _powerup == PowerupType.TripleShot ? FiringMode.TripleShot : FiringMode.HomingMissile;
                        collision.GetComponent<Player>().EnableWeapon(mode);
                        break;
                    case PowerupType.SpeedBoost:
                        collision.GetComponent<Player>().EnableSpeedBoost();
                        break;
                    case PowerupType.Shield:
                        collision.GetComponent<Player>().EnableShield();
                        break;
                    case PowerupType.AmmoCollectible:
                        collision.GetComponent<Player>().AddAmmo();
                        break;
                    case PowerupType.HealthCollectible:
                        collision.GetComponent<Player>().UpdateLives(1);
                        break;
                    case PowerupType.WeaponDisruption:
                        collision.GetComponent<Player>().EnableWeaponDistruption();
                        break;
                    case PowerupType.EnergyCollectible:
                        collision.GetComponent<Player>().EnableAcceleratedEnergyGain();
                     
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
        if (transform.position.y < GameManager.ENVIRONMENT_BOTTOM_BOUND - 0.01f)
            Destroy(gameObject);
        else
            transform.Translate(Vector2.down * _speed * Time.deltaTime);
    }

    public void GetDestroyed()
    {
        if(!_beingDestroyed)
        {
            _beingDestroyed = true;
            GetComponent<Collider2D>().enabled = false;
            Instantiate(_explosionPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }

    }
    #endregion
}
