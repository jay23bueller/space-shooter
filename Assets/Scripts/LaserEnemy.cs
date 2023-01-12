using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class LaserEnemy : Enemy
{
    #region Variables
    private WaitForSeconds _laserFireDelay = new WaitForSeconds(.03f);
    [SerializeField]
    private GameObject _chargingShotPrefab;
    [SerializeField]
    private AudioClip _laserBeamChargingClip;

    [SerializeField]
    private float _lineRendererZOffset = -.05f;

    private float _laserBeamChargingPositionOffset;

    [SerializeField]
    private float _segmentSize = 2f;

    [SerializeField]
    private float _additionalSegmentPieces = 2f;

    [SerializeField]
    private float _lineRendererPositionOffset = .2f;

    [SerializeField]
    private float _screenPositionDelta = 1.5f;


    private LineRenderer _lineRenderer;
    [SerializeField]
    private GameObject _weaponHitPrefab;
    private WaitForSeconds _laserBeamChargingWFS;
    private WaitForSeconds _playerHitWFS = new WaitForSeconds(3f);
    private WaitForSeconds _playerNotHitWFS = new WaitForSeconds(1.5f);
    private GameObject chargingGO;
    #endregion

    #region UnityMethods
    protected override void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _laserBeamChargingWFS = new WaitForSeconds(_laserBeamChargingClip.length - .1f);
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
        _lineRenderer.positionCount = 0;
        _audioSource.Stop();
        if(chargingGO != null)
        {
            Destroy(chargingGO);
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
                bool hurtPlayer = false;
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
                chargingGO = Instantiate(_chargingShotPrefab, transform.position + direction * _laserBeamChargingPositionOffset, Quaternion.identity);
                
                _audioSource.PlayOneShot(_laserBeamChargingClip);
                // wait for it to be over
                yield return _laserBeamChargingWFS;
                
                // then draw the line
                for(int i = 0; i < segments; i++)
                {
                    _lineRenderer.positionCount = i + 1;
                    direction.z = _lineRendererZOffset;
                    _lineRenderer.SetPosition(i, i > 1 ? _lineRenderer.GetPosition(i - 1) + (direction * 2f) : direction * _lineRendererPositionOffset  + transform.position);

                    
                    if (!hurtPlayer)
                    {
                        Vector3 currentPosition = _lineRenderer.GetPosition(i);
                        currentPosition.z = 0f;
                        float raycastDistance = (_lineRenderer.GetPosition(i) - transform.position).magnitude;
                        direction.z = 0f;
                        RaycastHit2D hit = Physics2D.CircleCast(transform.position, .05f, direction, raycastDistance, LayerMask.GetMask("Player"));
                        
                        if (hit.collider != null)
                        {
                            _player.UpdateLives(-1);
                            Instantiate(_weaponHitPrefab, hit.transform.position, Quaternion.identity);
                            hurtPlayer = true;
                        }


                    }

                    yield return _laserFireDelay;
                }


                

                if (!hurtPlayer)
                {
                    _lineRenderer.material.SetColor("_EmissionColor", Color.blue);
                    yield return _playerNotHitWFS;
                }

                else
                {
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
                _lineRenderer.material.SetColor("_EmissionColor", Color.red);
                _anim.speed = 1f;
                _canMove = true;
            }
            else
                break;
        }
    }

    private bool isWithinScreenBounds(Vector3 position)
    {
        Vector3 screenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));
        Vector3 screenBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f));

        return (position.x > screenBottomLeft.x && position.x < screenTopRight.x) && (position.y > screenBottomLeft.y && position.y < screenTopRight.y);
    }

    #endregion
}
