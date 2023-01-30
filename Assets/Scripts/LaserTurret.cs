using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    private enum FiringLaserBeamPhase
    {
        Ready,
        Charging,
        Drawing,
        Waiting,
        Cooldown,
        Resetting
    }
    #region Variables
    [SerializeField]
    private float _firingDelay = 1f;
    [SerializeField]
    private ParticleSystem _laserChargePS;
    [SerializeField]
    private float _laserFireDelay = .02f;
    [SerializeField]
    private float _maxLaserWidth = .07f;
    [SerializeField]
    private float _laserWidthDelta = .02f;
    [SerializeField]
    private float _waitingDelay = 2f;
    [SerializeField]
    private float _resetDelay = .02f;
    [SerializeField]
    private float _playerCooldownDelay = 2f;
    [SerializeField]
    private float _damageRadius = 2f;
    [SerializeField]
    private float _cooldownDelay = 2f;
    private float _cooldownTimer;
    private float _resetTimer;
    private float _waitingTimer;
    private AudioSource _audioSource;
    private bool _hitPlayer;
    private float _laserTimer;
    private float _firingTimer;
    private float _chargingTimer;
    private FiringLaserBeamPhase _phase;
    private LineRenderer[] _lineRenderers;
    private bool _canFire;
    public bool canFire { get => _canFire; }
    private Transform _playerTransform;
    private string[] _layerNames = new string[] { "Player", "Powerup" };
    #endregion

    #region UnityMethods

    private void Start()
    {
        _lineRenderers = GetComponentsInChildren<LineRenderer>();
        _audioSource = GetComponent<AudioSource>();

    }
    private void Update()
    {
        if (_canFire && _firingTimer < Time.time)
        {
            FireLaserBeam();
        }
    }

    #endregion
    #region Methods

    public void EnableFiring(bool firing) { _canFire = firing; }

    private void FireLaserBeam()
    {
        switch (_phase)
        {
            case FiringLaserBeamPhase.Ready:
                _chargingTimer = Time.time + _laserChargePS.main.duration;
                foreach (var lineRenderer in _lineRenderers)
                {
                    lineRenderer.widthMultiplier = _maxLaserWidth;
                }
                _audioSource.Play();
                _laserChargePS.Play();
                _phase = FiringLaserBeamPhase.Charging;
                break;
            case FiringLaserBeamPhase.Charging:
                if (!_audioSource.isPlaying)
                {
                    _laserChargePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    _phase = FiringLaserBeamPhase.Drawing;
                }
                break;
            case FiringLaserBeamPhase.Drawing:

                if (_laserTimer < Time.time)
                {
                    foreach (var lineRenderer in _lineRenderers)
                    {
                        if (lineRenderer.positionCount == 0)
                        {
                            lineRenderer.positionCount = 1;

                        }
                        else if (transform.TransformPoint(lineRenderer.GetPosition(lineRenderer.positionCount - 1)).y > GameManager.ENVIRONMENT_BOTTOM_BOUND)
                        {

                            lineRenderer.positionCount += 1;
                            lineRenderer.SetPosition(lineRenderer.positionCount - 1, lineRenderer.GetPosition(lineRenderer.positionCount - 2) + (Vector3.up * 2f));

                        }
                        else
                        {
                            if (_hitPlayer)
                            {
                                _cooldownTimer = Time.time + _playerCooldownDelay;
                                _phase = FiringLaserBeamPhase.Cooldown;
                            }
                            else
                            {
                                _phase = FiringLaserBeamPhase.Waiting;
                                _waitingTimer = Time.time + _waitingDelay;

                            }

                        }
                    }
                    AttemptToDamage();

                    _laserTimer = _laserFireDelay + Time.time;
                }



                break;
            case FiringLaserBeamPhase.Waiting:
                if (_waitingTimer > Time.time)
                {
                    AttemptToDamage();

                }
                else
                {
                    foreach (LineRenderer lineRenderer in _lineRenderers)
                    {
                        lineRenderer.material.color = Color.blue;
                        lineRenderer.material.SetColor("_EmissionColor", lineRenderer.material.color);
                    }

                    _phase = FiringLaserBeamPhase.Cooldown;
                    _cooldownTimer = Time.time + _cooldownDelay;
                }
                break;
            case FiringLaserBeamPhase.Cooldown:
                if (_cooldownTimer < Time.time)
                {
                    _resetTimer = _resetDelay + Time.time;
                    _phase = FiringLaserBeamPhase.Resetting;
                }
                break;
            case FiringLaserBeamPhase.Resetting:
                if (_resetTimer < Time.time)
                {
                    foreach (LineRenderer lineRenderer in _lineRenderers)
                    {
                        lineRenderer.widthMultiplier = Mathf.Clamp(lineRenderer.widthMultiplier - _laserWidthDelta, 0f, _maxLaserWidth);
                    }

                    if (_lineRenderers[0].widthMultiplier == 0f)
                    {
                        foreach (LineRenderer lineRenderer in _lineRenderers)
                        {
                            lineRenderer.positionCount = 0;
                            lineRenderer.material.color = Color.red;
                            lineRenderer.material.SetColor("_EmissionColor", lineRenderer.material.color);
                        }

                        _hitPlayer = false;


                        _firingTimer = _firingDelay + Time.time;
                        _phase = FiringLaserBeamPhase.Ready;
                    }
                    else
                        _resetTimer = _resetDelay + Time.time;

                }
                break;

        }
    }



    private void AttemptToDamage()
    {
        if (_hitPlayer) return;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _damageRadius, Vector3.down, Mathf.Abs(transform.position.y - transform.TransformPoint(_lineRenderers[0].GetPosition(_lineRenderers[0].positionCount - 1)).y), LayerMask.GetMask(_layerNames));
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<Player>().UpdateLives(-1);
                _hitPlayer = true;
                break;
            }


            if (hit.collider != null && hit.collider.CompareTag("Powerup"))
                hit.collider.GetComponent<Powerup>().GetDestroyed();

        }

        if (_hitPlayer)
        {
            foreach(var lineRenderer in _lineRenderers)
            {
                lineRenderer.material.color = Color.green;
                lineRenderer.material.SetColor("_EmissionColor", lineRenderer.material.color);
            }
        }
    }

    #endregion

}
