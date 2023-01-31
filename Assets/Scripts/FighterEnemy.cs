using System.Collections;
using UnityEngine;


public class FighterEnemy : MinionLaserEnemy
{

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

    //Shooting Laser at Powerup
    [Header("Shooting at Powerup")]
    [SerializeField]
    protected float _powerupShotAtDelay;
    [SerializeField]
    protected float _currentMinLaserPowerupDelay;
    [SerializeField]
    protected float _currentMaxLaserPowerupDelay;
    protected bool _shotAtPowerup;

    protected override IEnumerator FiringRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minFiringDelay, _maxFiringDelay));

            if (_currentMovement != null && _currentMovement is SelfDestructMovement) break;

            Vector3 directionToShoot = transform.up;
            

            GameObject laserGO = Instantiate(
                _laserPrefab,
                transform.position + (transform.up * 1f),
                  Quaternion.LookRotation(transform.forward, transform.up)
                );

            foreach (Laser laser in laserGO.GetComponentsInChildren<Laser>())
            {
                laser.InitializeFiring(0, false);
            };


            _beepingAudioSource.PlayOneShot(_laserAudioClip);

        }
    }
    private void Dodge()
    {

        if (_dodgingDistance < 0f)
        {
            _detectedLaserDelayTimer = _dodgedLaserDelay + Time.time;
            _isDodging = false;
            _thrusterGO.SetActive(false);

            if (_currentMovement != null && _currentMovement is ZigZagMovement)
            {
                ((ZigZagMovement)_currentMovement).UpdateZigZagX(transform.position.x);
            }

            if (_currentMovement != null && _currentMovement is IResetable)
            {
                ((IResetable)_currentMovement).Reset();
            }
           
            return;
        }
        _dodgingDistance -= _dodgingSpeed * Time.deltaTime;
        transform.Translate(_dodgingDirection * _dodgingSpeed * Time.deltaTime, Space.World);

    }

    public override void InitializeEnemy(Movement movement, float minFiringDelay, float maxFiringDelay, bool shieldEnabled)
    {

        base.InitializeEnemy(movement, minFiringDelay, maxFiringDelay, shieldEnabled);
        StartCoroutine(FireLaserPowerupRoutine());
    }

    private void DetectLaser()
    {

        if (!_isDodging && _detectedLaserDelayTimer < Time.time)
        {
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 2f, transform.up, 8f, LayerMask.GetMask("Laser"));

            if (hit.collider != null)
            {
                _dodgingDirection = Vector3.right * (Random.value > .5f ? 1f : -1f);
                _dodgingDistance = Random.Range(_minDodgingDistance, _maxDodgingDistance);
                _isDodging = true;
                _thrusterGO.SetActive(true);
                _beepingAudioSource.PlayOneShot(_chargingAudioClip);

            }

            _detectedLaserDelayTimer = _detectedLaserDelay + Time.time;
        }
    }
    protected virtual void Update()
    {
        if (_canMove)
        {
            Move();
            if (_currentMovement != null && !(_currentMovement is SelfDestructMovement))
                DetectLaser();
        }

    }

    protected override void Move()
    {
        base.Move();
        if (_isDodging)
        {
            Dodge();
        }
        else if (_currentMovement != null)
            _currentMovement.Move(transform);
    }



    private IEnumerator FireLaserPowerupRoutine()
    {
        while (true)
        {
            if (_shotAtPowerup)
                yield return new WaitForSeconds(_powerupShotAtDelay);
            else
                yield return new WaitForSeconds(Random.Range(_currentMinLaserPowerupDelay, _currentMaxLaserPowerupDelay));

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, .05f, transform.up, 20f, LayerMask.GetMask("Powerup"));

            if (hit.collider != null)
            {
                GameObject laser = Instantiate(_laserPrefab, transform.position + transform.up * 1f, transform.rotation);

                if (laser != null)
                {
                    laser.GetComponent<Laser>().InitializeFiring(0, false);
                }
                _shotAtPowerup = true;
            }
            else
                _shotAtPowerup = false;

        }
    }

}

public class CircularMovement : ReversibleMovement, IResetable
{

    public CircularMovement(float moveSpeed, float distanceFromTargetPosition, Vector3 diagonalEndPosition, Vector3 slantedDirection, Vector3 radiusEndPosition, float circularRotationSpeed, float radius, ReversibleState reversibleState) : base(moveSpeed, reversibleState)
    {
        this._distanceFromTargetPosition = distanceFromTargetPosition;
        this._diagonalEndPosition = diagonalEndPosition;
        this._slantedDirection = slantedDirection;
        this._radiusEndPosition = radiusEndPosition;
        this._circularRotationSpeed = circularRotationSpeed;
        this._radius = radius;
        _circularRadian = reversibleState == ReversibleState.Reverse ? 180f * Mathf.Deg2Rad : 0f;
    }
    public enum CircularMovementState
    {
        InitializeSlant,
        InitializeAwayFromCenter,
        Moving
    }

    private CircularMovementState _currentMovementState;

    private float _distanceFromTargetPosition = .15f;

    //InitializedSlant
    private Vector3 _diagonalEndPosition;
    private Vector3 _slantedDirection;

    //InitializeAwayFromCenter
    private Vector3 _radiusEndPosition;

    //Moving
    private float _circularRadian;
    private float _circularRotationSpeed;
    private float _radius;


    public override void Move(Transform transform)
    {
        base.Move(transform);

        switch (_currentMovementState)
        {
            case CircularMovementState.InitializeSlant:

                if (Vector3.Distance(transform.position, _diagonalEndPosition) > _distanceFromTargetPosition)
                {
                    transform.position += _slantedDirection * _moveSpeed * Time.deltaTime;
                }
                else
                    _currentMovementState = CircularMovementState.InitializeAwayFromCenter;
                break;

            case CircularMovementState.InitializeAwayFromCenter:

                if (Vector3.Distance(transform.position, _radiusEndPosition) > _distanceFromTargetPosition)
                {
                    transform.position += (_radiusEndPosition - transform.position).normalized * _moveSpeed * Time.deltaTime;
                }
                else
                    _currentMovementState = CircularMovementState.Moving;


                break;
            case CircularMovementState.Moving:
                _circularRadian = (_circularRadian + ((Mathf.Deg2Rad * _circularRotationSpeed) * Time.deltaTime)) % (Mathf.Deg2Rad * 360f);

                transform.position = (_radius * new Vector3(Mathf.Cos(_circularRadian), Mathf.Sin(_circularRadian))) + _diagonalEndPosition;
                break;
        }


    }

    public void Reset()
    {
        _currentMovementState = CircularMovementState.InitializeAwayFromCenter;
        _circularRadian = _currentReversibleState == ReversibleState.Reverse ? 180f * Mathf.Deg2Rad : 0f;
    }

}

public class HorizontalMovement : ReversibleMovement
{

    public HorizontalMovement(float moveSpeed, ReversibleState reversibleState) : base(moveSpeed, reversibleState) { }

    public override void Move(Transform transform)
    {
        base.Move(transform);
        transform.position += (int)_currentReversibleState * Vector3.right * _moveSpeed * Time.deltaTime;
    }
}

public class ZigZagMovement : ReversibleMovement, IResetable
{
    private float _zigZagCounter;
    private float _zigZagMaxDistance;
    private float _zigZagX;
    public ZigZagMovement(float moveSpeed, ReversibleState reversibleState, float zigZagMaxDistance, float zigZagX) : base(moveSpeed, reversibleState)
    {
        this._zigZagMaxDistance = zigZagMaxDistance;
        this._zigZagX = zigZagX;
    }

    public void UpdateZigZagX(float newZigZagX)
    {
        _zigZagX = newZigZagX;
    }

    public void Reset()
    {
        _zigZagCounter = 0;
    }

    public override void Move(Transform transform)
    {
        _zigZagCounter += _moveSpeed * Time.deltaTime;
        base.Move(transform);
        transform.position =
            new Vector3(Mathf.PingPong(_zigZagCounter, _zigZagMaxDistance) * (int)_currentReversibleState + _zigZagX,
            -_moveSpeed * Time.deltaTime + transform.position.y);
    }
}

public class VerticalMovement : Movement
{
    public VerticalMovement(float moveSpeed) : base(moveSpeed) { }
    public override void Move(Transform transform)
    {
        base.Move(transform);
        transform.position += Vector3.down * _moveSpeed * Time.deltaTime;
    }
}
