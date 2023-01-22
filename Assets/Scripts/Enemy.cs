using System.Collections;
using Random = UnityEngine.Random;
using UnityEngine;

public enum MovementMode
{
    WaypointVPath = 0,
    WaypointDiamondPath = 1,
    Horizontal,
    Vertical,
    ZigZag,
    Circular,
    PlayerTargeted
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

    [Header("Score")]
    [SerializeField]
    private int _normalScore = 10;
    [SerializeField]
    private int _shieldedScore = 50;

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



    [Header("Charging")]
    [SerializeField]
    private float _maxChargeSpeed = 8.0f;
    [SerializeField]
    private float _currentChargeSpeed = 3.5f;
    [SerializeField]
    private float _currentTime = 0f;
    [SerializeField]
    private float _distanceBeforeCharging = 4f;
    [SerializeField]
    private float _chargingAccelerationRate = 1.2f;
    [SerializeField]
    private GameObject _thrusterGO;
    [SerializeField]
    private AudioClip _chargingAudioClip;
    [SerializeField]
    private AudioSource _beepingAudioSource;
    [SerializeField]
    private float _chargeDelay;
    [SerializeField]
    private float _pitchIncrementDelay;
    [SerializeField]
    private float _pitchDelta = .02f;
    private float _pitchIncrementTimer;
    private float _chargeDelayTimer;
    protected bool _isEnraged;
    protected bool _initializeCharge;
    public bool isEnraged { get { return _isEnraged; } }
    protected bool _seekingPlayer;
    protected bool _chargingAtLastKnownPlayerLocation;
    private Vector3 _lastKnownDirectionToPlayer;



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

    //Player Targeted Movement
    private bool _playerTargetedMovement;
    private Vector3 _enemyToPlayerTargetedDirection;

    //Dodging
    [Header("Dodging")]
    [SerializeField]
    private float _dodgingSpeed = 10.0f;
    [SerializeField]
    private float _detectedLaserDelay;
    [SerializeField]
    private float _minDodgingDistance = 2.0f;
    [SerializeField]
    private float _maxDodgingDistance = 4.0f;
    [SerializeField]
    private float _dodgedLaserDelay = 1.5f;
    private float _detectedLaserDelayTimer;
    private bool _isDodging;
    private Vector3 _dodgingDirection;
    private float _dodgingDistance;
    private bool _dodgingEnabled;

    #endregion

    #region UnityMethods



    protected virtual void Start()
    {
        
        _anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWNMANAGER_TAG).GetComponent<SpawnManager>();

    }



    protected virtual void Update()
    {
        if(_canMove)
        {
            Move();
            if(_dodgingEnabled && !_isEnraged)
                DetectLaser();
        }
            
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && !_isDying)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                other.GetComponent<Player>().UpdateLives(-1);
                GetDestroyed(false);
            }
                
                
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other != null && !_isDying)
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

    private void EnragedSelfDestruct()
    {
        if(!_isDying)
            GetDestroyed(false);
    }

    public virtual void GetDestroyed(bool playerScored)
    {
        if(_isShieldEnabled)
        {
            StopCoroutine(FireLaser());
            _shieldGO.SetActive(false);
            _isShieldEnabled = false;
            _seekingPlayer = true;
            _anim.enabled = false;
            _isEnraged = true;
            _currentMovement = MoveEnraged;
            StartCoroutine(EnragedFlashRoutine());
            Invoke("EnragedSelfDestruct", 3f);
            return;
        }
        if (playerScored)
        {
            _player.AddScore(_isEnraged ? _shieldedScore : _normalScore);
            _wasKilled = true;
        }

        StopAllCoroutines();
        GetComponent<Collider2D>().enabled = false;
        _canMove = false;
        
        _speed = 0f;
        _thrusterGO.SetActive(false);
        _anim.enabled = true;
        _isDying = true;
        _beepingAudioSource.Stop();
        _anim.SetTrigger("OnEnemyDeath");
    }

    //Move the character to the bottom and if it is out of the viewport, teleport it to the top
    //at a random x location
    private void Move()
    {

        Teleport();

        if(_isDodging)
        {
            Dodge();
        }
        else
            _currentMovement();
    }

    private void DetectLaser()
    {
        
        if (!_isDodging && _detectedLaserDelayTimer < Time.time)
        {
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 2f, transform.up, 8f, LayerMask.GetMask("Laser"));

            if(hit.collider != null)
            {
                _dodgingDirection = Vector3.right * (Random.value > .5f ? 1f : -1f);
                _dodgingDistance = Random.Range(_minDodgingDistance, _maxDodgingDistance);
                _isDodging = true;
                _thrusterGO.SetActive(true);
                _audioSource.PlayOneShot(_chargingAudioClip);

            }

            _detectedLaserDelayTimer = _detectedLaserDelay + Time.time;
        }
    }

    private void Dodge()
    {

        if(_dodgingDistance < 0f)
        {
            _detectedLaserDelayTimer = _dodgedLaserDelay + Time.time;
            _isDodging = false;
            _thrusterGO.SetActive(false);
            _zigZagX = transform.position.x;
            _initializedDistanceAwayFromCenter = false;
            _circularRadian = 0f;
            return;
        }
        _dodgingDistance -= _dodgingSpeed * Time.deltaTime;
        transform.Translate(_dodgingDirection * _dodgingSpeed * Time.deltaTime, Space.World);

    }

    private void MovePlayerTargeted()
    {
        transform.Translate(_enemyToPlayerTargetedDirection * _speed * Time.deltaTime, Space.World);

    }

    protected void Teleport()
    {
        if (transform.position.y < GameManager.ENVIRONMENT_BOTTOM_BOUND)
        {
            transform.position = new Vector3(
                Random.Range(GameManager.LEFT_BOUND, GameManager.RIGHT_BOUND),
                GameManager.ENVIRONMENT_TOP_BOUND);
            
            _zigZagX = transform.position.x;

            InitializeTargetedMovement();
        }

        if (transform.position.x < GameManager.LEFT_BOUND)
            transform.position = new Vector3(GameManager.RIGHT_BOUND, transform.position.y);
        if (transform.position.x > GameManager.RIGHT_BOUND)
            transform.position = new Vector3(GameManager.LEFT_BOUND, transform.position.y);
    }

    protected void InitializeTargetedMovement()
    {
        if (_playerTargetedMovement && _player != null)
        {
            _enemyToPlayerTargetedDirection = (_player.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(transform.forward, _enemyToPlayerTargetedDirection);
        }
    }

    protected void MoveEnraged()
    {
        if (_player != null)
        {
            if (_seekingPlayer && _player != null)
            {
                if(_initializeCharge)
                {
                    
                    if (_pitchIncrementTimer < Time.time)
                    {
                        
                        _beepingAudioSource.pitch += _pitchDelta;
                        _pitchIncrementTimer = Time.time + _pitchIncrementDelay;
                    }
                }

                if (_initializeCharge && _chargeDelayTimer < Time.time)
                {
                    _seekingPlayer = false;
                        
                    _chargingAtLastKnownPlayerLocation = true;
                    _beepingAudioSource.Stop();
                    _audioSource.PlayOneShot(_chargingAudioClip);
                    
                    _thrusterGO.SetActive(true);
                }
                else
                {
                    Vector3 playerPosition = _player.transform.position;
                    _lastKnownDirectionToPlayer = (playerPosition - transform.position).normalized;


                    transform.Translate(_lastKnownDirectionToPlayer * Time.deltaTime * _currentChargeSpeed, Space.World);

                    if (!_initializeCharge && Vector3.Distance(transform.position, playerPosition) < _distanceBeforeCharging)
                    {
                        _initializeCharge = true;
                        _beepingAudioSource.Play();
                        _chargeDelayTimer = Time.time + _chargeDelay;

                    }
                }


                
            }

            if (_chargingAtLastKnownPlayerLocation)
            {
                _currentTime += Time.deltaTime * _chargingAccelerationRate;
                _currentChargeSpeed = Mathf.Clamp(Mathf.Exp(_currentTime) + _currentChargeSpeed, 0f, _maxChargeSpeed);


                transform.Translate(_lastKnownDirectionToPlayer * Time.deltaTime * _currentChargeSpeed, Space.World);
            }

            Quaternion enemyToPlayerRotation = Quaternion.LookRotation(transform.forward, _lastKnownDirectionToPlayer);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, enemyToPlayerRotation, 8f);

        }
    }

    protected bool IsChargingOrSeeking()
    {
        return _chargingAtLastKnownPlayerLocation || _seekingPlayer;
    }

    protected bool isWithinScreenBounds(Vector3 position)
    {
        Vector3 screenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));
        Vector3 screenBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f));

        return (position.x > screenBottomLeft.x && position.x < screenTopRight.x) && (position.y > screenBottomLeft.y && position.y < screenTopRight.y);
    }

    protected IEnumerator EnragedFlashRoutine()
    {
        Color flashingColor = Color.red;
        Color normalColor = Color.white;
        float frequency = .4f;
        bool isFlashing = false;
        
        while(!_isDying && !_chargingAtLastKnownPlayerLocation)
        {
            yield return new WaitForSeconds(frequency);
            frequency = Mathf.Clamp(frequency - .2f, 0f,.4f);
            isFlashing = !isFlashing;
            GetComponent<SpriteRenderer>().color = isFlashing ? flashingColor : normalColor;
        }

        if(!_isDying && _chargingAtLastKnownPlayerLocation)
        {
            GetComponent<SpriteRenderer>().color = flashingColor;
            
        }
    }

    public void SetMovementModeAndFiringDelays(MovementMode mode, bool isMirrored, float minFiringDelay, float maxFiringDelay, bool enableShield)
    {

        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG).GetComponent<Player>();
        if (_player == null) { Destroy(gameObject); return; }

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
            case MovementMode.PlayerTargeted:
                _playerTargetedMovement = true;
                InitializeTargetedMovement();
                _currentMovement = MovePlayerTargeted;
                break;

                
        }
        
        switch (mode)
        {
            case MovementMode.Horizontal:
            case MovementMode.ZigZag:
            case MovementMode.Vertical:
            case MovementMode.WaypointVPath:
            case MovementMode.WaypointDiamondPath:
            case MovementMode.Circular:
                if(mode != MovementMode.Vertical || mode != MovementMode.Circular)
                    _moveDirection = isMirrored ? -1 : 1;
                
                if (mode == MovementMode.WaypointVPath || mode == MovementMode.WaypointDiamondPath)
                {
                    _currentWaypointIndex = _waypointPaths[_waypointPathIndex].waypoints.Length - 1;
                }
                else
                {
                    StartCoroutine(FireLaserPowerupRoutine());
                    _detectedLaserDelayTimer = _detectedLaserDelay + Time.time;
                    _dodgingEnabled = true;
                }
                break;
        }


        StartCoroutine(FireLaser());
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

    private IEnumerator FireLaserPowerupRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(.9f,1.9f));

            RaycastHit2D hit  = Physics2D.CircleCast(transform.position, .05f, transform.up, 20f, LayerMask.GetMask("Powerup"));

            if (hit.collider != null)
            {
               GameObject laser = Instantiate(_laserPrefab, transform.position + transform.up * 1f, transform.rotation);

                if(laser != null)
                {
                    laser.GetComponent<Laser>().InitializeFiring(0, false);
                }
            }
                
        }
    }


    protected virtual IEnumerator FireLaser()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(_minFiringDelay, _maxFiringDelay));
            bool targetedShot = false;
            Vector3 directionToShoot = transform.up;
            if (_playerTargetedMovement && _player != null)
            {
                directionToShoot = (_player.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(-transform.up, directionToShoot);
               
                //At the moment hard-coded to be less than 45 degrees
                if (Mathf.Acos(dotProduct) < .78f)
                {
                    targetedShot = true;
                }
                else
                    continue;
            }


            GameObject laserGO = Instantiate(
                _laserPrefab, 
                transform.position + (targetedShot?(-transform.up * 1.2f) : (transform.up * 1f)), 
                  Quaternion.LookRotation(transform.forward, _playerTargetedMovement ? -transform.up : transform.up) 
                );
            
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
