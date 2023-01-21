using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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

    #endregion

    #region UnityMethods
    protected override void Start()
    {
        _playerHitWFS = new WaitForSeconds(_playerHitDelay/_playerHitDelaySegments);
        _playerNotHitWFS = new WaitForSeconds(_playerNoHitDelay/_playerHitDelaySegments);
        _laserFireDelayWFS = new WaitForSeconds(_laserFireDelay);
        _lineRenderer = GetComponent<LineRenderer>();
   
        _laserBeamChargingPositionOffset = GetComponent<CircleCollider2D>().radius;
        base.Start();
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
                Destroy(chargingEffectGO);
            }
        }

        
        base.GetDestroyed(playerScored);
    }

    protected override IEnumerator FireLaser()
    {
        while(true)
        {
            float currentFiringDelay = Random.Range(_minFiringDelay, _maxFiringDelay);

            WaitForSeconds currentFiringDelayWFS = new WaitForSeconds(currentFiringDelay / _laserStartDelaySegments);

            for(int i = 0; i < _laserStartDelaySegments; i++)
            {
                if(IsChargingOrSeeking())
                {
                    break;
                }

                yield return currentFiringDelayWFS;
            }
            if (IsChargingOrSeeking())
                break;
            if(_player != null)
            {
                _hurtPlayer = false;
                _anim.enabled = false;
                Vector3 playerPosition = _player.transform.position;
                Vector3 direction = (playerPosition - transform.position).normalized;

                Vector3 screenPosition = transform.position + direction * _laserBeamChargingPositionOffset;

                while(isWithinScreenBounds(screenPosition) && !IsChargingOrSeeking())
                {
                    screenPosition += direction * _screenPositionDelta;
                    yield return null;
                }

                if (IsChargingOrSeeking())
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
                while (_audioSource.isPlaying && !IsChargingOrSeeking())
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
                    if(IsChargingOrSeeking())
                        break;
                    _lineRenderer.positionCount = i + 1;
                    direction.z = _lineRendererZOffset;
                    _lineRenderer.SetPosition(i, i > 1 ? _lineRenderer.GetPosition(i - 1) + (direction * _segmentSize) : direction * _lineRendererPositionOffset  + transform.position);

                    AttemptToDamage(ref direction, i);

                    yield return _laserFireDelayWFS;
                }


                _attemptToHurtTimer = Time.time + _attemptToHurtDelay;
                
                while(!_hurtPlayer && Time.time < _attemptToHurtTimer && !IsChargingOrSeeking())
                {
                    AttemptToDamage(ref direction, _lineRenderer.positionCount - 1);
                    yield return null;
                }




                _lineRenderer.material.color = !_hurtPlayer ? Color.blue : Color.green;
                _lineRenderer.material.SetColor("_EmissionColor", _lineRenderer.material.color);

                WaitForSeconds currentWFS = !_hurtPlayer ? _playerNotHitWFS : _playerHitWFS;
                float currentSegments = !_hurtPlayer ? _playerNoHitDelaySegments : _playerHitDelaySegments;
                for(int i = 0; i < currentSegments; i++ )
                {
                    if (IsChargingOrSeeking())
                        break;

                    yield return currentWFS;
                }
                


                while(_lineRenderer.widthMultiplier > 0 && !IsChargingOrSeeking())
                {
                    _lineRenderer.widthMultiplier -= _widthDelta;
                    yield return null;
                }

                _lineRenderer.positionCount = 0;
                _lineRenderer.widthMultiplier = 1f;
                _lineRenderer.material.color = Color.red;
                _lineRenderer.material.SetColor("_EmissionColor", Color.red);
                _anim.enabled = !IsChargingOrSeeking() ? true : false;
                _canMove = true;
            }
            else
                break;
        }
    }



    private void AttemptToDamage(ref Vector3 direction, int lineRendererPosition)
    {

        if (!_hurtPlayer && !(_chargingAtLastKnownPlayerLocation || _seekingPlayer))
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
