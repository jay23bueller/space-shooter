using System.Collections;
using UnityEngine;


public abstract class BaseEnemy: MonoBehaviour
{
    
    protected string PLAYER_TAG = "Player";
    protected string SPAWNMANAGER_TAG = "SpawnManager";

    protected bool _justTeleported;
    [SerializeField]
    protected float _speed;
    protected SpawnManager _spawnManager;
    protected bool _isDying;
    public bool isDying { get => _isDying; }
    protected bool _canMove;

    protected float _minFiringDelay;
    protected float _maxFiringDelay;

    [Header("Shield")]
    [SerializeField]
    protected GameObject _shieldGO;

    protected Animator _anim;
    protected Player _player;

    [SerializeField]
    protected GameObject _thrusterGO;

    [SerializeField]
    protected int _scoreValue;

    protected Movement _currentMovement;
    protected bool _wasKilled;


    protected virtual void Awake()
    {
        _anim = GetComponent<Animator>();
        
    }
    protected abstract void Move();

    public virtual void InitializeEnemy(Movement movement, float minFiringDelay, float maxFiringDelay)
    {
        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG).GetComponent<Player>();
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWNMANAGER_TAG).GetComponent<SpawnManager>();

        if (_player == null) { Destroy(gameObject); return; }

        _minFiringDelay = minFiringDelay;
        _maxFiringDelay= maxFiringDelay;
        _currentMovement = movement;
        _canMove = true;
        StartCoroutine(FiringRoutine());
        GetComponent<Collider2D>().enabled = true;
    }
    protected virtual void Teleport()
    {

        if (transform.position.x < GameManager.LEFT_BOUND || transform.position.x > GameManager.RIGHT_BOUND || transform.position.y < GameManager.ENVIRONMENT_BOTTOM_BOUND)
            _justTeleported = true;

        if (transform.position.y < GameManager.ENVIRONMENT_BOTTOM_BOUND)
        {
            transform.position = new Vector3(
                Random.Range(GameManager.LEFT_BOUND - GameManager.SPAWN_LEFTRIGHT_OFFSET, GameManager.RIGHT_BOUND + GameManager.SPAWN_LEFTRIGHT_OFFSET),
                GameManager.ENVIRONMENT_TOP_BOUND);
        }

        if (transform.position.x < GameManager.LEFT_BOUND)
            transform.position = new Vector3(GameManager.RIGHT_BOUND, transform.position.y);
        if (transform.position.x > GameManager.RIGHT_BOUND)
            transform.position = new Vector3(GameManager.LEFT_BOUND, transform.position.y);
    }
    protected abstract IEnumerator FiringRoutine();

    public abstract void TakeDamage(bool playerScored);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && !_isDying)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                other.GetComponent<Player>().UpdateLives(-1);
                TakeDamage(false);
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
                TakeDamage(false);
            }


        }
    }

    protected virtual void DisableEnemy()
    {
        StopAllCoroutines();
        GetComponent<Collider2D>().enabled = false;
        _currentMovement = null;
        _speed = 0f;
        _thrusterGO.SetActive(false);
        _anim.enabled = true;
        _isDying = true;
        _anim.SetTrigger("OnEnemyDeath");
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public virtual void InformSpawnManager(float powerupSpawnDelayDuration)
    {
        _spawnManager.EnemyDestroyed(gameObject, powerupSpawnDelayDuration, _wasKilled, false);
    }
}

public abstract class Movement
{
    protected float _moveSpeed;
    public float moveSpeed { get => _moveSpeed; }

    public Movement(float moveSpeed)
    {
        this._moveSpeed = moveSpeed;
    }
    public virtual void Move(Transform transform)
    {
        if (transform == null) return;
    }

}

public class ReversibleMovement : Movement
{
    public enum ReversibleState
    {
        Normal = 1,
        Reverse = -1
    }
    protected ReversibleState _currentReversibleState;
    public ReversibleState currentReversibleState { get => _currentReversibleState; }

    public ReversibleMovement(float moveSpeed, ReversibleState reversibleState) : base(moveSpeed)
    {
        this._currentReversibleState = reversibleState;
    }

    public override void Move(Transform transform)
    {
        base.Move(transform);
    }
}

public class SelfDestructMovement : Movement
{

    public enum SelfDestructMovementState
    {
        Seeking,
        InitializeCharging,
        Charging
    }

    private Transform _target;
    private AudioSource _beepingAudioSource;
    private AudioClip _chargingAudioClip;
    private GameObject _thrusterGO;
    private SelfDestructMovementState _currentMovementState;
    public SelfDestructMovementState currentMovementState { get => _currentMovementState; }
    private Vector3 _lastKnownTargetDirection;
    private float _distanceBeforeCharging;
    private float _chargingDelayTimer;
    private float _chargingDelay;

    private float _pitchIncrementTimer;
    private float _pitchDelay;
    private float _pitchIncrement;

    private float _chargingElapsedTime;
    private float _chargingAccelerationRate;
    private float _currentChargeSpeed;
    private float _defaultChargeSpeed;
    private float _maxChargeSpeed;

    public SelfDestructMovement(float moveSpeed, ref AudioSource beepingAudioSource, ref Transform target, float distanceBeforeCharging, float chargingDelay, float pitchIncrement, float pitchDelay, ref GameObject thrusterGO, float chargingAccelerationRate, float defaultChargingSpeed, float maxChargeSpeed, AudioClip chargingAudioClip) : base(moveSpeed)
    {
        this._target = target;
        this._beepingAudioSource = beepingAudioSource;
        this._distanceBeforeCharging = distanceBeforeCharging;
        this._chargingDelay = chargingDelay;
        this._pitchIncrement = pitchIncrement;
        this._pitchDelay = pitchDelay;
        this._thrusterGO = thrusterGO;
        this._chargingAccelerationRate = chargingAccelerationRate;
        this._defaultChargeSpeed = defaultChargingSpeed;
        this._maxChargeSpeed = maxChargeSpeed;
        this._chargingAudioClip = chargingAudioClip;
    }

    public override void Move(Transform transform)
    {
        base.Move(transform);
        
        switch(_currentMovementState)
        {
            case SelfDestructMovementState.Seeking:
            case SelfDestructMovementState.InitializeCharging:
                Vector3 targetPosition = _target.position;
                _lastKnownTargetDirection = (targetPosition - transform.position).normalized;


                transform.Translate(_lastKnownTargetDirection * Time.deltaTime * _moveSpeed, Space.World);
                if (_currentMovementState == SelfDestructMovementState.Seeking)
                {
                    if (Vector3.Distance(transform.position, targetPosition) < _distanceBeforeCharging)
                    {
                        _currentMovementState = SelfDestructMovementState.InitializeCharging;
                        _beepingAudioSource.Play();
                        _chargingDelayTimer = Time.time + _chargingDelay;

                    }
                }
                if(_currentMovementState == SelfDestructMovementState.InitializeCharging)
                {
                    if (_chargingDelayTimer > Time.time)
                    {
                        if (_pitchIncrementTimer < Time.time)
                        {

                            _beepingAudioSource.pitch += _pitchIncrement;
                            _pitchIncrementTimer = Time.time + _pitchDelay;
                        }
                    }
                    else
                    {
                        _currentMovementState = SelfDestructMovementState.Charging;
                        _beepingAudioSource.Stop();
                        _beepingAudioSource.PlayOneShot(_chargingAudioClip);

                        _thrusterGO.SetActive(true);
                    }
                }
                
                break;
          
            case SelfDestructMovementState.Charging:
                _chargingElapsedTime += Time.deltaTime * _chargingAccelerationRate;

                _currentChargeSpeed = Mathf.Clamp(Mathf.Exp(_chargingElapsedTime) * .16f + _defaultChargeSpeed, 0f, _maxChargeSpeed);


                transform.Translate(_lastKnownTargetDirection * Time.deltaTime * _currentChargeSpeed, Space.World);
                break;
        }

        Quaternion enemyToPlayerRotation = Quaternion.LookRotation(transform.forward, _lastKnownTargetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, enemyToPlayerRotation, 8f);

        
    }
}

public interface IResetable
{
    public void Reset();
}

