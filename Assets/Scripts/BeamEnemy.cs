
using System.Collections;
using UnityEngine;


public class BeamEnemy : MinionEnemy
{
    //Charging
    [SerializeField]
    private GameObject _chargingShotPrefab;
    private GameObject chargingEffectGO;
    [SerializeField]
    private AudioClip _laserBeamChargingClip;
    private float _laserBeamChargingPositionOffset;
    [SerializeField]
    private AudioSource _audioSource;

    //Laser Beam
    [Header("Shooting Laser Beam")]
    [SerializeField]
    private float _lineRendererZOffset = -.05f;
    [SerializeField]
    private float _segmentSize = 2f;
    [SerializeField]
    private float _audioDelay;
    [SerializeField]
    private float _laserPitch;
    [SerializeField]
    private float _additionalSegmentPieces = 2f;
    [SerializeField]
    private float _lineRendererPositionOffset = .2f;
    [SerializeField]
    private float _screenPositionDelta = 1.5f;
    [SerializeField]
    private GameObject _weaponHitPrefab;
    [SerializeField]
    private float _laserFireDelay;
    [SerializeField]
    private float _laserStartDelaySegments;
    private WaitForSeconds _laserFireDelayWFS;
    private LineRenderer _lineRenderer;

    //Attempt to damage the player
    [Header("After Beam is shot")]
    [SerializeField]
    private float _attemptToHurtDelay = .05f;
    [SerializeField]
    private float _playerHitDelay;
    [SerializeField]
    private float _playerHitDelaySegments;
    [SerializeField]
    private float _playerNoHitDelay;
    [SerializeField]
    private float _playerNoHitDelaySegments;
    [SerializeField]
    private float _widthDelta = .02f;
    private WaitForSeconds _playerHitWFS;
    private WaitForSeconds _playerNotHitWFS;
    bool _hurtPlayer;
    private float _attemptToHurtTimer;
    private string[] _layerMaskNames = new string[] { "Player", "Powerup" };
    protected override IEnumerator FiringRoutine()
    {
        while (true)
        {
            float currentFiringDelay = Random.Range(_minFiringDelay, _maxFiringDelay);

            WaitForSeconds currentFiringDelayWFS = new WaitForSeconds(currentFiringDelay / _laserStartDelaySegments);

            for (int i = 0; i < _laserStartDelaySegments; i++)
            {
                if ((_currentMovement != null && _currentMovement is SelfDestructMovement))
                {
                    break;
                }

                yield return currentFiringDelayWFS;
            }
            if ((_currentMovement != null && _currentMovement is SelfDestructMovement))
                break;
            if (_player != null)
            {
                _hurtPlayer = false;
                _anim.enabled = false;
                Vector3 playerPosition = _player.transform.position;
                Vector3 direction = (playerPosition - transform.position).normalized;

                Vector3 screenPosition = transform.position + direction * _laserBeamChargingPositionOffset;

                while (isWithinScreenBounds(screenPosition) && !(_currentMovement != null && _currentMovement is SelfDestructMovement))
                {
                    screenPosition += direction * _screenPositionDelta;
                    yield return null;
                }

                if ((_currentMovement != null && _currentMovement is SelfDestructMovement))
                    break;

                float distance = Mathf.Ceil(Vector3.Distance(transform.position, screenPosition));
                float segments = Mathf.Ceil(distance / _segmentSize) + _additionalSegmentPieces;
                //stop moving
                _canMove = false;
                // instantiate the charging particle
                chargingEffectGO = Instantiate(_chargingShotPrefab, transform.position + direction * _laserBeamChargingPositionOffset, Quaternion.identity, transform);
                _audioSource.pitch = _laserPitch;
                _audioSource.PlayOneShot(_laserBeamChargingClip);
                // wait for it to be over
                while (_audioSource.isPlaying && !(_currentMovement != null && _currentMovement is SelfDestructMovement))
                    yield return null;



                if (chargingEffectGO != null)
                {
                    chargingEffectGO.GetComponent<ParticleSystem>().Stop();
                    Destroy(chargingEffectGO);
                }
                _audioSource.pitch = 0;
                // then draw the line
                for (int i = 0; i < segments; i++)
                {
                    if ((_currentMovement != null && _currentMovement is SelfDestructMovement))
                        break;
                    _lineRenderer.positionCount = i + 1;
                    direction.z = _lineRendererZOffset;
                    _lineRenderer.SetPosition(i, i > 1 ? _lineRenderer.GetPosition(i - 1) + (direction * _segmentSize) : direction * _lineRendererPositionOffset + transform.position);

                    AttemptToDamage(ref direction, i);

                    yield return _laserFireDelayWFS;
                }


                _attemptToHurtTimer = Time.time + _attemptToHurtDelay;

                while (!_hurtPlayer && Time.time < _attemptToHurtTimer && !(_currentMovement != null && _currentMovement is SelfDestructMovement))
                {
                    AttemptToDamage(ref direction, _lineRenderer.positionCount - 1);
                    yield return null;
                }




                _lineRenderer.material.color = !_hurtPlayer ? Color.blue : Color.green;
                _lineRenderer.material.SetColor("_EmissionColor", _lineRenderer.material.color);

                WaitForSeconds currentWFS = !_hurtPlayer ? _playerNotHitWFS : _playerHitWFS;
                float currentSegments = !_hurtPlayer ? _playerNoHitDelaySegments : _playerHitDelaySegments;
                for (int i = 0; i < currentSegments; i++)
                {
                    if ((_currentMovement != null && _currentMovement is SelfDestructMovement))
                        break;

                    yield return currentWFS;
                }



                while (_lineRenderer.widthMultiplier > 0 && !(_currentMovement != null && _currentMovement is SelfDestructMovement))
                {
                    _lineRenderer.widthMultiplier -= _widthDelta;
                    yield return null;
                }

                _lineRenderer.positionCount = 0;
                _lineRenderer.widthMultiplier = 1f;
                _lineRenderer.material.color = Color.red;
                _lineRenderer.material.SetColor("_EmissionColor", Color.red);
                _anim.enabled = !(_currentMovement != null && _currentMovement is SelfDestructMovement) ? true : false;
                _canMove = true;
            }
            else
                break;
        }
    }

    protected bool isWithinScreenBounds(Vector3 position)
    {
        Vector3 screenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));
        Vector3 screenBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f));

        return (position.x > screenBottomLeft.x && position.x < screenTopRight.x) && (position.y > screenBottomLeft.y && position.y < screenTopRight.y);
    }
    private void AttemptToDamage(ref Vector3 direction, int lineRendererPosition)
    {

        if (!_hurtPlayer && (_currentMovement == null) || !(_currentMovement != null && _currentMovement is SelfDestructMovement))
        {
            Vector3 currentPosition = _lineRenderer.GetPosition(lineRendererPosition);
            currentPosition.z = 0f;
            float raycastDistance = (_lineRenderer.GetPosition(lineRendererPosition) - transform.position).magnitude;
            direction.z = 0f;
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, .05f, direction, raycastDistance, LayerMask.GetMask(_layerMaskNames));

            if (hit.collider != null)
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    _player.UpdateLives(-1);
                    Instantiate(_weaponHitPrefab, hit.transform.position, Quaternion.identity);
                    _hurtPlayer = true;
                }
                else
                {
                    hit.collider.gameObject.GetComponent<Powerup>().GetDestroyed();
                }

            }


        }
    }
    protected virtual void Update()
    {
        if (_canMove)
        {
            Move();
        }

    }

    public override void TakeDamage(bool playerScored)
    {
        if(!_isShieldEnabled)
        {
            _lineRenderer.positionCount = 0;
            _audioSource.pitch = 0;
            _audioSource.Stop();
            _audioSource.pitch = 1;
            if (chargingEffectGO != null)
            {
                chargingEffectGO.GetComponent<ParticleSystem>().Stop();
                Destroy(chargingEffectGO);
            }
        }

        base.TakeDamage(playerScored);
    }

    protected override void Move()
    {
        base.Move();

        if (_currentMovement != null)
            _currentMovement.Move(transform);
    }

    public override void InitializeEnemy(Movement movement, float minFiringDelay, float maxFiringDelay, bool shieldEnabled)
    {
        _playerHitWFS = new WaitForSeconds(_playerHitDelay / _playerHitDelaySegments);
        _playerNotHitWFS = new WaitForSeconds(_playerNoHitDelay / _playerHitDelaySegments);
        _laserFireDelayWFS = new WaitForSeconds(_laserFireDelay);
        _lineRenderer = GetComponent<LineRenderer>();

        _laserBeamChargingPositionOffset = GetComponent<CircleCollider2D>().radius;
        base.InitializeEnemy(movement, minFiringDelay, maxFiringDelay, shieldEnabled);
    }
}
public class WaypointMovement : ReversibleMovement
{
    private Vector3[] _waypoints;
    private int _currentIdx;
    private float _distanceFromNextPoint;

    public WaypointMovement(float moveSpeed, ReversibleState reversibleState, float distanceFromNextPoint, Vector3[] waypoints) : base(moveSpeed, reversibleState)
    {
        this._distanceFromNextPoint = distanceFromNextPoint;
        this._waypoints = waypoints;

    }
    public override void Move(Transform transform)
    {
        base.Move(transform);
        if (Vector3.Distance(transform.position, _waypoints[_currentIdx]) < _distanceFromNextPoint)
        {
            _currentIdx = _currentIdx + (int)_currentReversibleState < 0 ? _waypoints.Length - 1 : _currentIdx + (int)_currentReversibleState;

            _currentIdx %= _waypoints.Length;

        }

        Vector3 waypointMoveDirection = (_waypoints[_currentIdx] - transform.position).normalized;

        transform.position += waypointMoveDirection * _moveSpeed * Time.deltaTime;

    }
}
