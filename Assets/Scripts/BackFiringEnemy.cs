
using System.Collections;
using UnityEngine;


public class BackFiringEnemy : MinionLaserEnemy
{
    [SerializeField]
    protected float _backFiringFOV = 60f;

    public void SetToSelfDestruct()
    {
        if(_isShieldEnabled)
        {
            TakeDamage(false);
        }
        
    }
    protected override IEnumerator FiringRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minFiringDelay, _maxFiringDelay));

            if (_currentMovement != null && _currentMovement is SelfDestructMovement) break;

            bool targetedShot = false;
            Vector3 directionToShoot = transform.up;
            if (_player != null)
            {
                directionToShoot = (_player.transform.position - transform.position).normalized;
                float dotProduct = Vector3.Dot(-transform.up, directionToShoot);
                if (Mathf.Acos(dotProduct) < _backFiringFOV * Mathf.Deg2Rad)
                {
                    targetedShot = true;
                }
                else
                    continue;
            }


            GameObject laserGO = Instantiate(
                _laserPrefab,
                transform.position + (targetedShot ? (-transform.up * 1.2f) : (transform.up * 1f)),
                  Quaternion.LookRotation(transform.forward,  -transform.up)
                );

            foreach (Laser laser in laserGO.GetComponentsInChildren<Laser>())
            {
                laser.InitializeFiring(0, false);
            };


            _beepingAudioSource.PlayOneShot(_laserAudioClip);

        }
    }

    public override void InitializeEnemy(Movement movement, float minFiringDelay, float maxFiringDelay, bool shieldEnabled)
    {
        //_justTeleported = true;
        base.InitializeEnemy(movement, minFiringDelay, maxFiringDelay, shieldEnabled);
        if(_player != null && _currentMovement != null && _currentMovement is DirectionMovement)
        {
            ((DirectionMovement)_currentMovement).UpdateMoveDirection(transform, (_player.transform.position - transform.position).normalized);
        }
    }

    protected virtual void Update()
    {
        if (_canMove)
        {
            Move();
        }

    }

    protected override void Move()
    {
        base.Move();

        if (_currentMovement != null)
            _currentMovement.Move(transform);
    }
}

public class DirectionMovement : Movement
{
    private Vector3 _moveDirection;
    public DirectionMovement(float moveSpeed, Vector3 moveDirection) : base(moveSpeed)
    {
        this._moveDirection = moveDirection;
    }
    public override void Move(Transform transform)
    {
        base.Move(transform);
        transform.Translate(_moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    public void UpdateMoveDirection(Transform transform, Vector3 direction)
    {
        _moveDirection = direction;
        transform.rotation = Quaternion.LookRotation(transform.forward, _moveDirection);
    }
}
