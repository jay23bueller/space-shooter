using System.Collections;
using UnityEngine;

public class BossEnemy : Enemy
{
    private enum BossPhase
    {
        Initializing,
        PhaseOne,
        PhaseTwo,
        PhaseThree,
        Destroyed
    }
    #region Variables
    [Header("Boss")]
    [SerializeField]
    private float _bossPhaseOneFiringDelay = 2f;
    [SerializeField]
    private float _bossPhaseTwoFiringDelay = 1.2f;
    [SerializeField]
    private float _bossPhaseThreeFiringDelay = 1f;
    [SerializeField]
    private float _bossHealth = 100f;
    [SerializeField]
    private GameObject _backfiringEnemyForBoss;
    [SerializeField]
    private float _bossInitializingSpeedMultiplier = .5f;
    [SerializeField]
    private float _bossDamageAmount = 2f;

    private float _currentBossFiringDelay;
    private BossPhase _currentBossPhase;
    private bool _initializedTurrets;

    #endregion


    #region UnityMethods
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }
    #endregion

    #region Methods
    private IEnumerator BossFiringRoutine()
    {
        while (_currentBossPhase != BossPhase.Destroyed)
        {
            if(_currentBossPhase != BossPhase.Initializing)
            {
                for (int i = 0; i < 5; i++)
                {
                    yield return new WaitForSeconds(_currentBossFiringDelay);
                    GameObject backFiringEnemy = Instantiate(_backfiringEnemyForBoss, new Vector3(transform.position.x, transform.position.y - 2f), transform.rotation, transform.parent);
                    if (backFiringEnemy != null) { backFiringEnemy.GetComponent<Enemy>().SetMovementModeAndFiringDelays(MovementMode.PlayerTargeted, false, 1f, 3f, true); }
                    if (backFiringEnemy != null) { backFiringEnemy.GetComponent<Enemy>().TakeDamage(false); }

                    _spawnManager.AddEnemy(backFiringEnemy);

                }
            }

           

            yield return null;
        }
    }

    private void UpdateBossPhase(BossPhase bossPhase)
    {
        switch(bossPhase)
        {
            case BossPhase.PhaseTwo:
                _currentBossFiringDelay = _bossPhaseTwoFiringDelay;
                _currentBossPhase = BossPhase.PhaseTwo;
                if (!_initializedTurrets)
                {
                    foreach (var turret in GetComponentsInChildren<Turret>()) { turret.EnableFiring(true); }
                    _initializedTurrets = true;
                }
                break;
            case BossPhase.PhaseThree:
                _currentBossFiringDelay = _bossPhaseThreeFiringDelay;
                _currentBossPhase = BossPhase.PhaseThree;
                if (!GetComponentInChildren<LaserTurret>().canFire)
                    GetComponentInChildren<LaserTurret>().EnableFiring(true);
                break;
        }
    }

    private void MoveBoss()
    {
        switch (_currentBossPhase)
        {
            case BossPhase.Initializing:
                if (Mathf.Abs(transform.position.y - (GameManager.ENVIRONMENT_TOP_BOUND - (GameManager.ENVIRONMENT_TOP_BOUND - GameManager.ENVIRONMENT_BOTTOM_BOUND) * .2f)) < .1f)
                {
                    _currentBossFiringDelay = _bossPhaseOneFiringDelay;
                    _currentBossPhase = BossPhase.PhaseOne;

                }

                transform.Translate(Vector3.down * _speed * _bossInitializingSpeedMultiplier * Time.deltaTime, Space.World);
                break;
            case BossPhase.PhaseThree:
            
                    if (Mathf.Abs(GameManager.LEFT_BOUND - (transform.position.x - GetComponent<BoxCollider2D>().bounds.extents.x)) < .1f || Mathf.Abs(GameManager.RIGHT_BOUND - (transform.position.x + GetComponent<BoxCollider2D>().bounds.extents.x)) < .1f)
                    {
                        _moveDirection = -_moveDirection;
                    }
                transform.Translate(Vector3.right * _moveDirection * _speed * Time.deltaTime, Space.World);



                break;


        }
    }


    public override void TakeDamage(bool playerScored)
    {
        if (_isIntialized)
        {
           
            if (_currentBossPhase != BossPhase.Initializing)
            {
                _bossHealth = Mathf.Clamp(_bossHealth - _bossDamageAmount, 0f, 100f);
                var shieldColor = _shieldGO.GetComponent<SpriteRenderer>().color;
                shieldColor.g = _bossHealth * .01f;
                shieldColor.b = _bossHealth * .01f;
                _shieldGO.GetComponent<SpriteRenderer>().color = shieldColor;
                if (_bossHealth < 66f && _currentBossPhase == BossPhase.PhaseOne)
                {
                    UpdateBossPhase(BossPhase.PhaseTwo);
                    

                } 
                else if(_bossHealth < 33f && _currentBossPhase == BossPhase.PhaseTwo)
                {
                    UpdateBossPhase(BossPhase.PhaseThree);
            }
            }

        }

        if (_bossHealth == 0f)
        {
            _shieldGO.SetActive(false);
            if (playerScored)
            {
                _player.AddScore(_normalScore);
                _wasKilled = true;
            }


            foreach (LaserTurret laserTurret in GetComponentsInChildren<LaserTurret>())
                laserTurret.gameObject.SetActive(false);

            foreach (Turret turret in GetComponentsInChildren<Turret>())
                turret.gameObject.SetActive(false);

            

            DisableEnemy();
        }
    }

    public override void SetMovementModeAndFiringDelays(MovementMode mode, bool isMirrored, float minFiringDelay, float maxFiringDelay, bool enableShield)
    {
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWNMANAGER_TAG).GetComponent<SpawnManager>();
        _currentMovementMode = mode;
        _player = GameObject.FindGameObjectWithTag(PLAYER_TAG).GetComponent<Player>();
        if (_player == null) { Destroy(gameObject); return; }

        if (enableShield)
        {
            _shieldGO.SetActive(true);
            _isShieldEnabled = true;
        }
        _minFiringDelay = minFiringDelay;
        _maxFiringDelay = maxFiringDelay;
       
        _currentMovement = MoveBoss;
        _currentFiringRoutine = BossFiringRoutine;
        

        _isIntialized = true;
        StartCoroutine(_currentFiringRoutine());
        _canMove = true;
    }
    public override void InformSpawnManager(float powerupSpawnDelayDuration)
    {
        _spawnManager.EnemyDestroyed(gameObject, powerupSpawnDelayDuration, _wasKilled, true);
    }

    #endregion
}



