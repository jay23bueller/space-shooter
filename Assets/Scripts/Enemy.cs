using System.Collections;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    #region Constants
    private const string PLAYER_TAG = "Player";
    private const string LASER_TAG = "Laser";
    private const string ENEMY_TAG = "Enemy";
    private const string POWERUP_TAG = "Powerup";
    private const string ENEMYLASER_TAG = "EnemyLaser";
    #endregion

    #region Variables
    [SerializeField]
    private float _speed = 4.0f;
    private Rigidbody2D _rigidbody;
    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _laserAudioClip;
    [SerializeField]
    private GameObject _laserPrefab;
    #endregion

    #region UnityMethods



    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG).GetComponent<Player>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        StartCoroutine(FireLaser());
        _rigidbody.velocity = Vector2.down * _speed;

    }



    private void FixedUpdate()
    {
        Teleport();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null)
        {
            if (other.CompareTag(PLAYER_TAG))
                other.GetComponent<Player>().TakeDamage();


            if (other.CompareTag(LASER_TAG))
            {
                if(_player != null)
                    _player.AddScore(10);
                Destroy(other.gameObject);
            }
                

            if (!other.CompareTag(ENEMY_TAG) && !other.CompareTag(POWERUP_TAG) && !other.CompareTag(ENEMYLASER_TAG))
            {
                GetComponent<Collider2D>().enabled = false;
                _rigidbody.velocity = Vector2.zero;
                StopAllCoroutines();
                _speed = 0f;
                _anim.SetTrigger("OnEnemyDeath");
            }
                
        }
    }

    #endregion

    #region Methods

    //Move the character to the bottom and if it is out of the viewport, teleport it to the top
    //at a random x location
    private void Teleport()
    {
        Vector3 currentViewportPosition = Camera.main.WorldToViewportPoint(_rigidbody.position);
        if (currentViewportPosition.y < SpawnManager.BOTTOM_BOUND)
        {
            _rigidbody.position = Camera.main.ViewportToWorldPoint(
                new Vector2(Random.Range(SpawnManager.LEFT_BOUND, SpawnManager.RIGHT_BOUND),
                SpawnManager.TOP_BOUND));
        }
    }

    private IEnumerator FireLaser()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 7f));


            GameObject laserGO = Instantiate(_laserPrefab, transform.position, transform.rotation);
            foreach(Laser laser in laserGO.GetComponentsInChildren<Laser>())
            {
                laser.InitializeFiring(0);
            };

            
            _audioSource.PlayOneShot(_laserAudioClip);
            
        }
    }

    public void Die()
    {
        Destroy(gameObject);

    }
    #endregion
}
