using System.Collections;
using UnityEngine;

public class MinionEnemy : BaseEnemy
{

    [SerializeField]
    protected int _shieldScoreValue;

    protected bool _isShieldEnabled;

    [SerializeField]
    protected AudioSource _beepingAudioSource;
    [SerializeField]
    protected AudioClip _chargingAudioClip;

    protected override void Teleport()
    {
        base.Teleport();
        if(_justTeleported)
        {
            if (_currentMovement != null && _currentMovement is ZigZagMovement)
            {
                ((ZigZagMovement)_currentMovement).UpdateZigZagX(transform.position.x);
            }
            if (_currentMovement != null && _currentMovement is DirectionMovement && _player != null)
            {
                ((DirectionMovement)_currentMovement).UpdateMoveDirection(transform, (_player.transform.position - transform.position).normalized);
            }
        }
        _justTeleported = false;

    }
    protected override IEnumerator FiringRoutine()
    {
        yield return null;
    }

    public override void TakeDamage(bool playerScored)
    {
        if (_isShieldEnabled)
        {
            StopCoroutine(FiringRoutine());
            _shieldGO.SetActive(false);
            _isShieldEnabled = false;

            _anim.enabled = false;
            _currentMovement = null;
            var tempTransform = _player.transform;
            _currentMovement = new SelfDestructMovement(
                _spawnManager.selfDestructMovementInfo.moveSpeed,
                ref _beepingAudioSource,
                ref tempTransform,
                _spawnManager.selfDestructMovementInfo.distanceBeforeCharging,
                _spawnManager.selfDestructMovementInfo.chargingDelay,
                _spawnManager.selfDestructMovementInfo.pitchIncrement,
                _spawnManager.selfDestructMovementInfo.pitchDelay,
                ref _thrusterGO,
                _spawnManager.selfDestructMovementInfo.chargingAccelerationRate,
                _spawnManager.selfDestructMovementInfo.defaultChargeSpeed,
                _spawnManager.selfDestructMovementInfo.maxChargeSpeed, 
                _chargingAudioClip);
            _canMove = true;
            StartCoroutine(SelfDestructFlashRoutine());
            Invoke("SelfDestruct", 3f);
            return;
        }



        if (playerScored)
        {
            _player.AddScore(_currentMovement is SelfDestructMovement ? _shieldScoreValue : _scoreValue);
            _wasKilled = true;
        }



        DisableEnemy();

    }
    protected override void Move()
    {
        Teleport();

    }

    protected void SelfDestruct()
    {
        if (!_isDying)
            TakeDamage(false);
    }

    protected override void DisableEnemy()
    {
        _beepingAudioSource.Stop();
        base.DisableEnemy();
    }

    public virtual void InitializeEnemy(Movement movement, float minFiringDelay, float maxFiringDelay, bool shieldEnabled)
    {
        if (shieldEnabled)
        {
            _shieldGO.SetActive(true);
            _isShieldEnabled = true;
        }

        base.InitializeEnemy(movement, minFiringDelay, maxFiringDelay);

    }



    protected IEnumerator SelfDestructFlashRoutine()
    {
        Color flashingColor = Color.red;
        Color normalColor = Color.white;
        float frequency = .4f;
        bool isFlashing = false;
        var selfDestructMovement = (SelfDestructMovement)_currentMovement;
        while (!_isDying && selfDestructMovement.currentMovementState == SelfDestructMovement.SelfDestructMovementState.InitializeCharging)
        {
            yield return new WaitForSeconds(frequency);
            frequency = Mathf.Clamp(frequency - .2f, 0f, .4f);
            isFlashing = !isFlashing;
            GetComponent<SpriteRenderer>().color = isFlashing ? flashingColor : normalColor;
        }

        if (!_isDying)
        {
            GetComponent<SpriteRenderer>().color = flashingColor;

        }
    }
}
