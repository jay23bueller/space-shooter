using System.Collections;
using UnityEngine;


public class FinalBossEnemy : BaseEnemy
{
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
    private float _bossDamageAmount = 2f;

    private float _currentBossFiringDelay;
    private BossPhase _currentBossPhase;
    private bool _initializedTurrets;
    private enum BossPhase
    {
        Initializing,
        PhaseOne,
        PhaseTwo,
        PhaseThree,
        Destroyed
    }

    protected virtual void Update()
    {
        if(_canMove)
        {
            if (_currentMovement != null)
                _currentMovement.Move(transform);
        }
    }

    public override void TakeDamage(bool playerScored)
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
            else if (_bossHealth < 33f && _currentBossPhase == BossPhase.PhaseTwo)
            {
                UpdateBossPhase(BossPhase.PhaseThree);
            }
        }

    

        if (_bossHealth == 0f)
        {
            _shieldGO.SetActive(false);
            if (playerScored)
            {
                _player.AddScore(_scoreValue);
                _wasKilled = true;
            }


            foreach (LaserTurret laserTurret in GetComponentsInChildren<LaserTurret>())
                laserTurret.gameObject.SetActive(false);

            foreach (Turret turret in GetComponentsInChildren<Turret>())
                turret.gameObject.SetActive(false);



        DisableEnemy();
        }
    }

    protected override void Move()
    {
        if (_currentMovement != null)
            _currentMovement.Move(transform);
    }

    protected override IEnumerator FiringRoutine()
    {
        while (_currentBossPhase != BossPhase.Destroyed)
        {
            if (_currentBossPhase != BossPhase.Initializing)
            {
                for (int i = 0; i < 5; i++)
                {
                    yield return new WaitForSeconds(_currentBossFiringDelay);
                    GameObject backFiringEnemy = Instantiate(_backfiringEnemyForBoss, new Vector3(transform.position.x, transform.position.y - 2f), transform.rotation, transform.parent);

                    backFiringEnemy.GetComponent<BackFiringEnemy>().InitializeEnemy(new DirectionMovement(_spawnManager.playerTargetedMovementInfo.moveSpeed, Vector3.zero),
                        1f, 1f, true
                        );
                    if (backFiringEnemy != null) backFiringEnemy.GetComponent<BackFiringEnemy>().SetToSelfDestruct();

                    _spawnManager.AddEnemy(backFiringEnemy);

                }
            }



            yield return null;
        }
    }

    private void UpdateBossPhase(BossPhase bossPhase)
    {
        switch (bossPhase)
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
                ((BossMovement)_currentMovement).UpdateCurrentBossMovementPhase(BossMovement.BossMovementPhase.Aggressive);
                if (!GetComponentInChildren<LaserTurret>().canFire)
                    GetComponentInChildren<LaserTurret>().EnableFiring(true);
                break;
        }
    }

    public void StartAttacking()
    {
        _currentBossFiringDelay = _bossPhaseOneFiringDelay;
        _currentBossPhase = BossPhase.PhaseOne;
    }

    public override void InformSpawnManager(float powerupSpawnDelayDuration)
    {
        _spawnManager.EnemyDestroyed(gameObject, powerupSpawnDelayDuration, _wasKilled, false);
    }

}

public class BossMovement : ReversibleMovement
{
    public enum BossMovementPhase
    {
        Initializing,
        Idle,
        Aggressive
    }

    private float _bossInitializingSpeedMultiplier;

    public BossMovement(float moveSpeed, float bossInitializingSpeedMultiplier) :base(moveSpeed,ReversibleState.Normal)
    {
        _bossInitializingSpeedMultiplier = bossInitializingSpeedMultiplier;
    }

    public void UpdateCurrentBossMovementPhase(BossMovementPhase bossMovementPhase)
    {
        _currentMovementPhase = bossMovementPhase;
    }

    private BossMovementPhase _currentMovementPhase;
    public override void Move(Transform transform)
    {
        switch (_currentMovementPhase)
        {
            case BossMovementPhase.Initializing:
                if (Mathf.Abs(transform.position.y - (GameManager.ENVIRONMENT_TOP_BOUND - (GameManager.ENVIRONMENT_TOP_BOUND - GameManager.ENVIRONMENT_BOTTOM_BOUND) * .2f)) < .1f)
                {
                    
                    _currentMovementPhase = BossMovementPhase.Idle;
                    transform.GetComponent<FinalBossEnemy>().StartAttacking();

                }

                transform.Translate(Vector3.down * _moveSpeed * _bossInitializingSpeedMultiplier * Time.deltaTime, Space.World);
                break;
            case BossMovementPhase.Aggressive:

                if (Mathf.Abs(GameManager.LEFT_BOUND - (transform.position.x - transform.GetComponent<BoxCollider2D>().bounds.extents.x)) < .1f || Mathf.Abs(GameManager.RIGHT_BOUND - (transform.position.x + transform.GetComponent<BoxCollider2D>().bounds.extents.x)) < .1f)
                {
                    _currentReversibleState = (ReversibleState)(-(int)_currentReversibleState);
                }
                transform.Translate(Vector3.right * (int)_currentReversibleState * _moveSpeed * Time.deltaTime, Space.World);



                break;


        }
    }
}
