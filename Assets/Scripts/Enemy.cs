using System.Collections;
using UnityEngine;

public enum MovementMode
{
    Horizontal,
    Vertical,
    ZigZag,
    Circular
}

public class Enemy : MonoBehaviour
{
    #region Constants
    private const string PLAYER_TAG = "Player";
    private const string LASER_TAG = "Laser";
    private const string ENEMY_TAG = "Enemy";
    private const string POWERUP_TAG = "Powerup";
    private const string ENEMYLASER_TAG = "EnemyLaser"; //enums, tags are stored as an array, can use specific indexes
    private const string SPAWNMANAGER_TAG = "SpawnManager";
    #endregion

    #region Variables
    [SerializeField]
    private float _speed = 4.0f;
    private Player _player;
    private Animator _anim;
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _laserAudioClip;
    [SerializeField]
    private GameObject _laserPrefab;
    private SpawnManager _spawnManager;
    private bool _wasKilled;
    private delegate void Movement();
    private Movement _currentMovement;
    private bool _canMove;

    //Circular Movement
    private Vector3 _diagonalStartPosition;
    private Vector3 _diagonalEndPosition;
    [SerializeField]
    private float _radius;
    private Vector3 _slantedDirection;
    private bool _initializedCircularSlant;
    private bool _initializedDistanceAwayFromCenter;
    private Vector3 _radiusEndPosition;
    private float _circularRadian;
    [SerializeField]
    private float _circularRotationSpeed = 40f;
    [SerializeField]
    private float _leftSlant = -30f;
    [SerializeField]
    private float _rightSlant = -150f;
    [SerializeField]
    private float _distanceFromSlant = 10f;

    //ZigZag Movement
    private float _zigZagX;
    private float _zigZagCounter;
    [SerializeField]
    private float _zigZagMaxDistance = 3f;

    private float _moveDirection;
    #endregion

    #region UnityMethods



    void Start()
    {
        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG).GetComponent<Player>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWNMANAGER_TAG).GetComponent<SpawnManager>();
        StartCoroutine(FireLaser());

    }



    private void Update()
    {
        if(_canMove)
            Move();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null)
        {
            if (other.CompareTag(PLAYER_TAG))
                other.GetComponent<Player>().UpdateLives(-1);


            if (other.CompareTag(LASER_TAG))
            {
                if(_player != null)
                    _player.AddScore(10);
                _wasKilled = true;
                Destroy(other.gameObject);
            }
                

            if (!other.CompareTag(ENEMY_TAG) && !other.CompareTag(POWERUP_TAG) && !other.CompareTag(ENEMYLASER_TAG))
            {
                GetComponent<Collider2D>().enabled = false;
                _canMove = false;
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
    private void Move()
    {
        

        if (transform.position.y < GameManager.ENVIRONMENT_BOTTOM_BOUND)
        {
            transform.position = new Vector3(
                Random.Range(GameManager.LEFT_BOUND, GameManager.RIGHT_BOUND),
                GameManager.ENVIRONMENT_TOP_BOUND);
            _zigZagX = transform.position.x;
        }

        if(transform.position.x < GameManager.LEFT_BOUND)
            transform.position = new Vector3(GameManager.RIGHT_BOUND, transform.position.y);
        if (transform.position.x > GameManager.RIGHT_BOUND)
            transform.position = new Vector3(GameManager.LEFT_BOUND, transform.position.y);

        _currentMovement();
    }

    public void SetMovementMode(MovementMode mode, bool isMirrored)
    {
        switch(mode)
        {
            case MovementMode.Horizontal:
                _currentMovement = MoveHorizontally;
                break;
            case MovementMode.Circular:
                _diagonalStartPosition = new Vector3(isMirrored ? GameManager.RIGHT_BOUND : GameManager.LEFT_BOUND, GameManager.ENVIRONMENT_TOP_BOUND);
                _diagonalEndPosition = (Quaternion.AngleAxis(isMirrored ? _rightSlant : _leftSlant, Vector3.forward) * Vector3.right * _distanceFromSlant) + _diagonalStartPosition;
                _slantedDirection = (_diagonalEndPosition - _diagonalStartPosition).normalized;
                _radiusEndPosition = _diagonalEndPosition + ((isMirrored ? Vector3.left : Vector3.right) * _radius);
                _circularRotationSpeed *= isMirrored ? -1f : 1f;
                _circularRadian = isMirrored ? 180f * Mathf.Deg2Rad : 0f;
                _currentMovement = MoveCircular;
                break;
            case MovementMode.Vertical:
                _currentMovement = MoveVertically;
                break;
            case MovementMode.ZigZag:
                _zigZagX = transform.position.x;
                _currentMovement = MoveZigZag;
                break;

                
        }
        
        if(mode == MovementMode.ZigZag || mode == MovementMode.Horizontal)
            _moveDirection = isMirrored ? -1 : 1;

        _canMove = true;
    }

    private void MoveHorizontally()
    {
        transform.position += _moveDirection * Vector3.right * _speed * Time.deltaTime;
    }

    private void MoveVertically()
    {
        transform.position += Vector3.down * _speed * Time.deltaTime;
    }

    private void MoveZigZag()
    {
        _zigZagCounter += _speed * Time.deltaTime;
        transform.position = 
            new Vector3(Mathf.PingPong(_zigZagCounter, _zigZagMaxDistance)*_moveDirection + _zigZagX,
            -_speed * Time.deltaTime + transform.position.y) ;
    }

    //First moves into the game world at an angle
    //Then moves away from the target position
    //and starts rotating around the target position
    //a specific distance away
    private void MoveCircular()
    {
        if(!_initializedCircularSlant)
        {
            if (Vector3.Distance(transform.position, _diagonalEndPosition) > .15f)
            {
                transform.position += _slantedDirection * _speed * Time.deltaTime;
            }
            else
                _initializedCircularSlant = true;
        } 
        else if(!_initializedDistanceAwayFromCenter)
        {
            if (Vector3.Distance(transform.position, _radiusEndPosition) > .15f)
            {
                transform.position += (_radiusEndPosition - transform.position).normalized * _speed * Time.deltaTime;
            }
            else
            {
                _initializedDistanceAwayFromCenter = true;
                
            }
                
        }
        else
        {
            _circularRadian = (_circularRadian + ((Mathf.Deg2Rad * _circularRotationSpeed)*Time.deltaTime)) % (Mathf.Deg2Rad * 360f);
          
            transform.position = (_radius * new Vector3(Mathf.Cos(_circularRadian), Mathf.Sin(_circularRadian))) + _diagonalEndPosition;
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

    public void InformSpawnManager(float powerupSpawnDelayDuration)
    {
        if(_wasKilled)
        _spawnManager.EnemyDestroyed(gameObject, powerupSpawnDelayDuration);
    }

    public void Die()
    {
        Destroy(gameObject);
    }
    #endregion
}
