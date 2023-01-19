using System;
using System.Collections;
using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.UIElements;

public enum MovementMode
{
    WaypointVPath = 0,
    WaypointDiamondPath = 1,
    Horizontal,
    Vertical,
    ZigZag,
    Circular,

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
    protected float _speed = 4.0f;

    protected delegate void Movement();
    protected Movement _currentMovement;
    protected bool _canMove;
    private float _moveDirection;
    protected bool _isDying;
    public bool isDying { get => _isDying; }

    [Header("Firing")]
    [SerializeField]
    protected float _minFiringDelay;
    [SerializeField]
    protected float _maxFiringDelay;
    [SerializeField]
    protected GameObject _laserPrefab;
    [SerializeField]
    protected AudioClip _laserAudioClip;

    protected SpawnManager _spawnManager;
    protected AudioSource _audioSource;
    protected bool _wasKilled;
    protected Player _player;
    protected Animator _anim;

    //Shield
    [Header("Shield")]
    [SerializeField]
    private GameObject _shieldGO;
    protected bool _isShieldEnabled;



    //Circular Movement
    [Header("Circular Movement")]
    [SerializeField]
    private float _radius;
    [SerializeField]
    private float _circularRotationSpeed = 40f;
    [SerializeField]
    private float _leftSlant = -30f;
    [SerializeField]
    private float _rightSlant = -150f;
    [SerializeField]
    private float _distanceFromSlant = 10f;
    private Vector3 _diagonalStartPosition;
    private Vector3 _diagonalEndPosition;
    private Vector3 _slantedDirection;
    private bool _initializedCircularSlant;
    private bool _initializedDistanceAwayFromCenter;
    private Vector3 _radiusEndPosition;
    private float _circularRadian;

    //ZigZag Movement
    [Header("ZigZag Movement")]
    [SerializeField]
    private float _zigZagMaxDistance = 3f;
    private float _zigZagX;
    private float _zigZagCounter;




    //Waypoint Movement
    [Header("Waypoint Movement")]
    [SerializeField]
    private WaypointPathInfo[] _waypointPaths;
    private int _currentWaypointIndex;
    private int _waypointPathIndex;
    #endregion

    #region UnityMethods



    protected virtual void Start()
    {
        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG).GetComponent<Player>();
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWNMANAGER_TAG).GetComponent<SpawnManager>();
        StartCoroutine(FireLaser());

    }



    protected virtual void Update()
    {
        if(_canMove)
        {
            Move();

            //if(!_isShieldEnabled && Time.time > _shieldDestroyedDelayTimer)
            //{

            //}
        }
            
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                other.GetComponent<Player>().UpdateLives(-1);
                GetDestroyed(false);
            }
                
                
        }
    }


    #endregion

    #region Methods

    public virtual void GetDestroyed(bool playerScored)
    {
        if(_isShieldEnabled)
        {
          
            _shieldGO.SetActive(false);
            _isShieldEnabled = false;
            return;
        }
        if (playerScored)
        {
            _player.AddScore(10);
            _wasKilled = true;
        }

        StopAllCoroutines();
        GetComponent<Collider2D>().enabled = false;
        _canMove = false;
        
        _speed = 0f;
        _anim.speed = 1f;
        _isDying = true;
        _anim.SetTrigger("OnEnemyDeath");
    }

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

    public void SetMovementModeAndFiringDelays(MovementMode mode, bool isMirrored, float minFiringDelay, float maxFiringDelay, bool enableShield)
    {
        if (enableShield)
        {
            _shieldGO.SetActive(true);
            _isShieldEnabled = true;
        }
        _minFiringDelay = minFiringDelay;
        _maxFiringDelay = maxFiringDelay;
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
            case MovementMode.WaypointDiamondPath:
            case MovementMode.WaypointVPath:
                _currentWaypointIndex = 0;
                _waypointPathIndex = (int)mode;
                _currentMovement = MoveWaypointPath;
                break;

                
        }
        
        switch (mode)
        {
            case MovementMode.Horizontal:
            case MovementMode.ZigZag:
            case MovementMode.WaypointVPath:
            case MovementMode.WaypointDiamondPath:
                _moveDirection = isMirrored ? -1 : 1;
                if(mode == MovementMode.WaypointVPath || mode == MovementMode.WaypointDiamondPath) 
                {
                    _currentWaypointIndex = _waypointPaths[_waypointPathIndex].waypoints.Length - 1;
                }
                break;
        }

        

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


    protected virtual IEnumerator FireLaser()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(_minFiringDelay, _maxFiringDelay));


            GameObject laserGO = Instantiate(_laserPrefab, transform.position + (transform.up * 1f), transform.rotation);
            foreach(Laser laser in laserGO.GetComponentsInChildren<Laser>())
            {
                laser.InitializeFiring(0, false);
            };

            
            _audioSource.PlayOneShot(_laserAudioClip);
            
        }
    }

    private void MoveWaypointPath()
    {
        if (Vector3.Distance(transform.position, _waypointPaths[_waypointPathIndex].waypoints[_currentWaypointIndex]) < .1f)
        {
            int newIndex = (_currentWaypointIndex + (int)_moveDirection);
            _currentWaypointIndex =  newIndex < 0 ? _waypointPaths[_waypointPathIndex].waypoints.Length - 1 : newIndex % _waypointPaths[_waypointPathIndex].waypoints.Length;

            
        }
        Vector3 waypointMoveDirection = (_waypointPaths[_waypointPathIndex].waypoints[_currentWaypointIndex] - transform.position).normalized;

        transform.position += waypointMoveDirection * _speed * Time.deltaTime;
    }

    public void InformSpawnManager(float powerupSpawnDelayDuration)
    {
        _spawnManager.EnemyDestroyed(gameObject, powerupSpawnDelayDuration,_wasKilled);
    }

    public void Die()
    {
        Destroy(gameObject);
    }
    #endregion
}
