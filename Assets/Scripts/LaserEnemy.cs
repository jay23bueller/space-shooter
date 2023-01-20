using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class LaserEnemy : Enemy
{
    #region Variables

    //Charging
    [SerializeField]
    private GameObject _chargingShotPrefab;
    private GameObject chargingEffectGO;
    [SerializeField]
    private AudioClip _laserBeamChargingClip;
    private float _laserBeamChargingPositionOffset;

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
    private WaitForSeconds _laserFireDelayWFS;
    private LineRenderer _lineRenderer;

    //Attempt to damage the player
    [Header("After Beam is shot")]
    [SerializeField]
    private float _attemptToHurtDelay = .05f;
    [SerializeField]
    private float _playerHitDelay;
    [SerializeField]
    private float _playerNoHitDelay;
    private WaitForSeconds _playerHitWFS;
    private WaitForSeconds _playerNotHitWFS;
    bool _hurtPlayer;
    private float _attemptToHurtTimer;

    #endregion

    #region UnityMethods
    protected override void Start()
    {
        _playerHitWFS = new WaitForSeconds(_playerHitDelay);
        _playerNotHitWFS = new WaitForSeconds(_playerNoHitDelay);
        _laserFireDelayWFS = new WaitForSeconds(_laserFireDelay);
        _lineRenderer = GetComponent<LineRenderer>();
   
        _laserBeamChargingPositionOffset = GetComponent<CircleCollider2D>().radius;
        base.Start();
    }

    private void OnAnimatorMove()
    {
        if(!_isDying && _seekingPlayer)
        {
            if (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime % _anim.GetCurrentAnimatorStateInfo(0).length < 0.05f)
                _anim.speed = 0f;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    #endregion

    #region Methods
    public override void GetDestroyed(bool playerScored)
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
            }
        }

        
        base.GetDestroyed(playerScored);
    }

    protected override IEnumerator FireLaser()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(_minFiringDelay, _maxFiringDelay));
            if(_player != null)
            {
                _hurtPlayer = false;
                _anim.speed = 0;
                Vector3 playerPosition = _player.transform.position;
                Vector3 direction = (playerPosition - transform.position).normalized;

                Vector3 screenPosition = transform.position + direction * _laserBeamChargingPositionOffset;

                while(isWithinScreenBounds(screenPosition))
                {
                    screenPosition += direction * _screenPositionDelta;
                    yield return null;
                }
                float distance = Mathf.Ceil(Vector3.Distance(transform.position, screenPosition));
                float segments = Mathf.Ceil(distance / _segmentSize) + _additionalSegmentPieces;
                //stop moving
                _canMove = false;
                // instantiate the charging particle
                chargingEffectGO = Instantiate(_chargingShotPrefab, transform.position + direction * _laserBeamChargingPositionOffset, Quaternion.identity, transform);
                _audioSource.pitch = _laserPitch;
                _audioSource.PlayOneShot(_laserBeamChargingClip);
                // wait for it to be over
                while (_audioSource.isPlaying)
                    yield return null;
                
                if(chargingEffectGO != null) 
                { 
                    chargingEffectGO.GetComponent<ParticleSystem>().Stop();
                    Destroy(chargingEffectGO);
                }
                _audioSource.pitch = 0;
                // then draw the line
                for (int i = 0; i < segments; i++)
                {
                    _lineRenderer.positionCount = i + 1;
                    direction.z = _lineRendererZOffset;
                    _lineRenderer.SetPosition(i, i > 1 ? _lineRenderer.GetPosition(i - 1) + (direction * _segmentSize) : direction * _lineRendererPositionOffset  + transform.position);

                    AttemptToDamage(ref direction, i);

                    yield return _laserFireDelayWFS;
                }

                _attemptToHurtTimer = Time.time + _attemptToHurtDelay;
                
                while(!_hurtPlayer && Time.time < _attemptToHurtTimer)
                {
                    AttemptToDamage(ref direction, _lineRenderer.positionCount - 1);
                    yield return null;
                }
                

                if (!_hurtPlayer)
                {
                    _lineRenderer.material.color = Color.blue;
                    _lineRenderer.material.SetColor("_EmissionColor", Color.blue);
                    yield return _playerNotHitWFS;
                }

                else
                {
                    _lineRenderer.material.color = Color.green;
                    _lineRenderer.material.SetColor("_EmissionColor", Color.green);
                    yield return _playerHitWFS;
                }
                
                
                while(_lineRenderer.widthMultiplier > 0)
                {
                    _lineRenderer.widthMultiplier -= .02f;
                    yield return null;
                }

                _lineRenderer.positionCount = 0;
                _lineRenderer.widthMultiplier = 1f;
                _lineRenderer.material.color = Color.red;
                _lineRenderer.material.SetColor("_EmissionColor", Color.red);
                _anim.speed = 1f;
                _canMove = true;
            }
            else
                break;
        }
    }



    private void AttemptToDamage(ref Vector3 direction, int lineRendererPosition)
    {

        if (!_hurtPlayer)
        {
            Vector3 currentPosition = _lineRenderer.GetPosition(lineRendererPosition);
            currentPosition.z = 0f;
            float raycastDistance = (_lineRenderer.GetPosition(lineRendererPosition) - transform.position).magnitude;
            direction.z = 0f;
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, .05f, direction, raycastDistance, LayerMask.GetMask("Player"));

            if (hit.collider != null)
            {
                _player.UpdateLives(-1);
                Instantiate(_weaponHitPrefab, hit.transform.position, Quaternion.identity);
                _hurtPlayer = true;
            }


        }
    }

    #endregion
}
