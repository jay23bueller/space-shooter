using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum FiringMode
{
    Default,
    TripleShot,
    HomingMissile,
    Shotgun
    
}
public class Player : MonoBehaviour
{
    #region Constants
    private const string VERTICAL_AXIS = "Vertical";
    private const string HORIZONTAL_AXIS = "Horizontal";
    private const string SPAWN_MANAGER_TAG = "SpawnManager";
    #endregion

    #region Variables

    private Animator _anim;

    //Movement
    [Header("Movement")]
    [SerializeField]
    private float _speed = 3.5f;
    [SerializeField]
    private float _speedMultiplier = 1.0f;
    private float _defaultSpeedMultiplier = 1.0f;


    //Thruster
    [Header("Thrusters")]
    [SerializeField]
    private float _thrusterBoostMultiplier = 1.3f;
    [SerializeField]
    private GameObject _thrusterGO;
    [SerializeField]
    private float _defaultThrusterDrainRate = -2.5f;
    [SerializeField]
    private float _maxThrusterDrainRate = 300f;
    [SerializeField]
    private float _thrusterDrainRateDecrement = 0f;
    [SerializeField]
    private float _currentThrusterDrainRate;
    [SerializeField]
    private float _thrusterGainRate = 2.5f;
    [SerializeField]
    private float _thrusterAcceleratedGainRate = 6f;
    [SerializeField]
    private float _thrusterAcceleratedGainRateDuration = 4f;
    [SerializeField]
    private float _thrusterCurrentEnergy;
    [SerializeField]
    private float _thrusterRecoveryMultiplier = 2.0f;
    [SerializeField]
    private AudioClip _outOfThrusterEnergyClip;
    [SerializeField]
    private float _thrusterDisabledSoundInterval = .3f;
    [SerializeField]
    private AudioClip _thrusterBoostClip;
    private float _currentThrusterGainRate;
    private bool _punishPlayer;
    private bool _fullyCharged;
    private bool _justPunished;
    private bool _playBoostSound = true;
    private bool _engagingThrusters;
    private float _delayThrusterDisabledSoundTimer;
    private float _thrusterMaxEnergy = 100f;
    private float _thrusterMinEnergy = 0f;
    private Coroutine _thrusterAcceleratedGainCooldownRoutine;
    private WaitForSeconds _thrusterAcceleratedGainCooldownWFS;



    [Header("Powerup")]
    [SerializeField]
    private float _powerupCooldownDuration = 5;
    [SerializeField]
    private float _powerupCooldownDurationIncrement = .3f;

    //SpeedBoost
    [Header("Speed Boost")]
    [SerializeField]
    private float _speedBoostMultipler = 2.0f;
    private Coroutine _resetSpeedBoostCoroutine;
    private bool _isSpeedBoostEnabled;


    //TripleShot
    [Header("Triple Shot")]
    [SerializeField]
    private GameObject _tripleShotPrefab;
    private Coroutine _resetWeaponRountine;


    //Current Weapon
    [Header("Weapon")]
    [SerializeField]
    private int _ammoMaxCount = 15;
    [SerializeField]
    private AudioClip _outOfAmmoClip;
    private FiringMode _firingMode;
    private int _ammoCurrentCount = 15;
    private bool _canFire = true;
    private float _ammoDelayBeforeSpawningTimer;
    [SerializeField]
    private float _ammoDelayBeforeSpawning = 5.0f;

    //Laser
    [Header("Laser")]
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private Transform _laserSpawnTransform;
    [SerializeField]
    private float _laserCooldownDuration = .2f;
    [SerializeField]
    private AudioClip _laserAudioClip;

    //Missile
    [Header("Missile")]
    [SerializeField]
    private GameObject _missilePrefab;
    [SerializeField]
    private AudioClip _missileAudioClip;
    [SerializeField]
    private float _missileCooldownDuration = 1f;
    private float _weaponCooldownDuration;

    [Header("Shotgun")]
    [SerializeField]
    float _angleDisplacement = 25f;
    [SerializeField]
    int _numberOfShotgunLasers = 5;
    [SerializeField]
    private float _shotgunCooldownDuration = .4f;
    private float _startAngle;
    private float _totalAngleCoverage;


    //Shield
    [Header("Shield")]
    [SerializeField]
    private GameObject _shieldGO;
    [SerializeField]
    private Color _shieldFullHealthColor;
    [SerializeField]
    private Color _shieldMediumHealthColor;
    [SerializeField]
    private Color _shieldLowHealthColor;
    private int _shieldMaxHealth = 3;
    private int _shieldCurrentHealth;
    private bool _isShieldEnabled;

    //Managers
    [Header("Manager Info")]
    [SerializeField]
    private UIManager _uiManager;
    [SerializeField]
    private AudioManager _audioManager;
    private SpawnManager _spawnManager;


    //Score
    [Header("Score")]
    [SerializeField]
    private int _healthDropScoreDivisor = 200;
    private int _score;

    //Effects
    [Header("Effects")]
    [SerializeField]
    private GameObject[] _engines;
    [SerializeField]
    private GameObject _explosionGO;
    [SerializeField]
    private AudioClip _weaponCooldownOverSound;
    private AudioSource _audioSource;



    //Lives
    [Header("Lives")]
    [SerializeField]
    private int _lives = 3;
    [SerializeField]
    private AudioClip _playerLostLifeClip;
    private int _maxLives = 3;

    [Header("Magnet")]
    [SerializeField]
    private ParticleSystem _magneticPS;
    [SerializeField]
    private AudioSource _magnetAudioSource;
    [SerializeField]
    private AudioClip _magnetDeactivationClip;
    [SerializeField]
    private AudioClip _magnetReadyClip;
    [SerializeField]
    private float _magnetDuration = 3f;
    [SerializeField]
    private float _magneticRadius = 10f;
    [SerializeField]
    private float _magnetResetDuration = 3f;
    private float _magnetTimer;
    public MagnetState playerMagnetState { get => _magnetState; }
    private MagnetState _magnetState;
    public enum MagnetState
    {
        Ready,
        Using,
        Resetting
    }

    private bool _disruptWeapon;

    #endregion

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {

        //Set starting position
        transform.position = new Vector3(0f, 0f, 0f);
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWN_MANAGER_TAG).GetComponent<SpawnManager>();
        _ammoCurrentCount = _ammoMaxCount;
        _currentThrusterGainRate = _thrusterGainRate;
        _uiManager.UpdateScoreText(_score, false);
        _uiManager.SetAmmoMaxCount(_ammoMaxCount);
        _uiManager.UpdateAmmoText(_ammoCurrentCount);
        _uiManager.SetMagnetUIDuration(_magnetResetDuration);
        _thrusterAcceleratedGainCooldownWFS = new WaitForSeconds(_thrusterAcceleratedGainRateDuration);
        _anim = GetComponent<Animator>();
        if (_spawnManager == null)
            Debug.LogError("The Spawn Manager is NULL");

        if (_uiManager == null)
            Debug.LogError("The UI Manager is NULL");

        _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
            Debug.LogError("Player missing AudioSource component!");

        _thrusterCurrentEnergy = _thrusterMaxEnergy;
        _currentThrusterDrainRate = _defaultThrusterDrainRate;

        _magneticPS = GetComponent<ParticleSystem>();

        _startAngle = -90f + _angleDisplacement;
        float endAngle = 90f - _angleDisplacement;
        _totalAngleCoverage = endAngle - _startAngle;
    }
    

    // Update is called once per frame
    void Update()
    {
        if(_lives != 0)
        {
            CheckThrusterPower();
            CheckForThrusterInput();
            MoveCharacter();
            UpdateThrusterUI();
            FireWeapon();
            CheckAmmoCount();
            CheckMagnet();
        }


    }


    #endregion

    #region Methods
    private void CheckMagnet()
    {
        switch (_magnetState)
        {
            case MagnetState.Ready:
                if (Input.GetKeyDown(KeyCode.O))
                {
                    _magnetState = MagnetState.Using;
                    _magnetTimer = _magnetDuration + Time.time;
                    _magneticPS.Play();
                    _magnetAudioSource.Play();
                    _uiManager.UpdateMagnetImageState(UIManager.MagnetUIState.Pulsing);
                    RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _magneticRadius, Vector2.zero, 0f, LayerMask.GetMask("Powerup"));

                    foreach (var hit in hits)
                    {
                        if (hit.collider != null)
                            hit.collider.GetComponent<Powerup>().targetTransform = transform;
                    }
                }
                break;
            case MagnetState.Using:
                if (_magnetTimer < Time.time)
                {
                    _magneticPS.Stop();
                    _magnetAudioSource.Stop();
                    _magnetState = MagnetState.Resetting;
                    _uiManager.UpdateMagnetImageState(UIManager.MagnetUIState.Vanish);
                    _magnetTimer = _magnetResetDuration + Time.time;
                    _magnetAudioSource.PlayOneShot(_magnetDeactivationClip);
                }
                break;
            case MagnetState.Resetting:
                if (_magnetTimer < Time.time)
                {
                    _magnetState = MagnetState.Ready;
                    _magnetAudioSource.PlayOneShot(_magnetReadyClip);
                }
                break;
        }
    }


    private void CheckForThrusterInput()
    {
        if (Input.GetKey(KeyCode.J))
        {
            if (!_punishPlayer && !_justPunished)
            {
                _engagingThrusters = true;
                GetComponent<Collider2D>().enabled = false;
                
                if(_playBoostSound)
                {
                    _playBoostSound = false;
                    _audioSource.PlayOneShot(_thrusterBoostClip);
                }
            }             
            else if(Time.time > _delayThrusterDisabledSoundTimer)
            {
                _delayThrusterDisabledSoundTimer = _thrusterDisabledSoundInterval + Time.time;
                _audioSource.PlayOneShot(_outOfThrusterEnergyClip);
            }
                
        }
            
        if (Input.GetKeyUp(KeyCode.J))
        {
            _engagingThrusters = false;
            GetComponent<Collider2D>().enabled = true;
            _playBoostSound = true;
            
        }
            

    }

    public void EnableWeaponDistruption()
    {
        if(!_disruptWeapon)
        {
            _disruptWeapon = true;
            _uiManager.UpdateDisruptionText(true);
            StartCoroutine(ResetPowerup(PowerupType.WeaponDisruption));
        }
            
    }


    private void UpdateThrusterUI()
    {
        _uiManager.UpdateThrusterSlider(_thrusterCurrentEnergy, _justPunished);
    }

    private IEnumerator PunishPlayerRoutine()
    {
        _punishPlayer = true;
        yield return new WaitForSeconds(2f);
        _punishPlayer = false;

    }

    private void CheckThrusterPower()
    {
        if(_thrusterCurrentEnergy == _thrusterMinEnergy && !_punishPlayer && !_justPunished)
        {
            _justPunished = true;
            GetComponent<Collider2D>().enabled = true;
            StartCoroutine(PunishPlayerRoutine());
        }
        if(!_fullyCharged && !_punishPlayer)
        {

            float batteryCharge = Time.deltaTime * _currentThrusterGainRate * ((_thrusterCurrentEnergy < .5f * _thrusterMaxEnergy) && _justPunished ? _thrusterRecoveryMultiplier : 1f);

            _thrusterCurrentEnergy = Mathf.Clamp(batteryCharge + _thrusterCurrentEnergy, _thrusterMinEnergy, _thrusterMaxEnergy);
            if (_justPunished && _thrusterCurrentEnergy >= .5f * _thrusterMaxEnergy)
            {
                _justPunished = false;
            }
            if (_thrusterCurrentEnergy == _thrusterMaxEnergy)
                _fullyCharged = true;
        }
    }

    private void SetBoost()
    {
        if (_isSpeedBoostEnabled)
        {
            _speedMultiplier = _speedBoostMultipler;
        }
        else
        {
            _speedMultiplier = _defaultSpeedMultiplier;
        }


        if (_engagingThrusters && !_justPunished)
        {
            _speedMultiplier = _thrusterBoostMultiplier;
            _thrusterCurrentEnergy = Mathf.Clamp(_thrusterCurrentEnergy + (_currentThrusterDrainRate * Time.deltaTime), _thrusterMinEnergy, _thrusterMaxEnergy);
            _fullyCharged = false;
        }
    }

    //Move the character based on input within the viewport
    private void MoveCharacter()
    {
        SetBoost();

        //The thruster gameobject should give a visual feedback for the movement speed
        _thrusterGO.transform.localScale = new Vector3(_speedMultiplier,1f,1f);

        Vector3 verticalDirection = Vector3.up * Input.GetAxis(VERTICAL_AXIS) * _speed * _speedMultiplier * Time.deltaTime;
        Vector3 horizontalDirection = Vector3.right * Input.GetAxis(HORIZONTAL_AXIS) * _speed * _speedMultiplier * Time.deltaTime;
        _anim.SetFloat("Direction", Input.GetAxis(HORIZONTAL_AXIS));
        Vector2 nextVerticalViewportPosition = transform.position + verticalDirection;
        Vector2 nextHorizontalViewportPosition = transform.position + horizontalDirection;

        //If the character's new position is within the top and bottom bounds, then move it
        if (nextVerticalViewportPosition.y < GameManager.PLAYER_TOP_BOUND && nextVerticalViewportPosition.y > GameManager.PLAYER_BOTTOM_BOUND)
            transform.Translate(verticalDirection);

        //If the character's new position is outside the left or right bounds, teleport the character
        if (nextHorizontalViewportPosition.x > GameManager.RIGHT_BOUND)
        {
            transform.position = new Vector3(GameManager.LEFT_BOUND, nextHorizontalViewportPosition.y);
        }
        else if (nextHorizontalViewportPosition.x < GameManager.LEFT_BOUND)
        {
            transform.position = new Vector3(GameManager.RIGHT_BOUND, nextHorizontalViewportPosition.y);
        }

        transform.Translate(horizontalDirection);

    }

    public void UpdateAmmoCapacityAndResetCurrentAmmo(int additionalAmmo)
    {
        _ammoMaxCount += additionalAmmo;
        _ammoCurrentCount = _ammoMaxCount;
        _uiManager.SetAmmoMaxCount(_ammoMaxCount);
        _uiManager.UpdateAmmoText(_ammoCurrentCount);
    }

    public void UpdatePowerupDuration(int waveIndex)
    {
        _powerupCooldownDuration += _powerupCooldownDurationIncrement * waveIndex;
        _uiManager.UpdateWeaponCooldownDuration(_powerupCooldownDuration);
    }

    private void CheckAmmoCount()
    {
        if(_spawnManager.waveStarted && _ammoCurrentCount == 0 && _ammoDelayBeforeSpawningTimer < Time.time)
        {
            _spawnManager.SpawnAmmoCollectible();
            _ammoDelayBeforeSpawningTimer = Time.time + _ammoDelayBeforeSpawning;
            
        }
    }

    public void UpdateThrusterDrainRate(int streakLevel)
    {
        _currentThrusterDrainRate = Mathf.Clamp(_thrusterDrainRateDecrement * streakLevel + _defaultThrusterDrainRate,_defaultThrusterDrainRate, _maxThrusterDrainRate);
    }

    //Attempt to fire weapon
    private void FireWeapon()
    {
        if(_canFire && Input.GetKeyDown(KeyCode.I))
        {
            if(_ammoCurrentCount > 0)
            {
                _ammoCurrentCount--;
                _uiManager.UpdateAmmoText(_ammoCurrentCount);

                switch (_firingMode)
                {
                    case FiringMode.Default:
                    case FiringMode.TripleShot:
                    case FiringMode.Shotgun:
                        SpawnLaser(_firingMode);
                        break;

                    case FiringMode.HomingMissile:
                        Missile missile = Instantiate(_missilePrefab, _laserSpawnTransform.position, _laserSpawnTransform.rotation).GetComponent<Missile>();
                        missile.InitializeFiring(1, _disruptWeapon);
                        _audioSource.PlayOneShot(_missileAudioClip);
                        break;
                }
                
                _canFire = false;
                StartCoroutine(ResetWeaponCooldown());
            }
            else
            {
                _audioSource.PlayOneShot(_outOfAmmoClip);
            }
   

        }
            
    }

    private void SpawnLaser(FiringMode mode)
    {
        List<Laser> lasers = new List<Laser>();
        switch (mode)
        {
            case FiringMode.Default:
                lasers.Add(Instantiate(_laserPrefab, _laserSpawnTransform.position, _laserSpawnTransform.rotation).GetComponent<Laser>());
                break;
            case FiringMode.TripleShot:
                lasers.AddRange(Instantiate(_tripleShotPrefab, transform.position, transform.rotation).GetComponentsInChildren<Laser>());
                break;
            case FiringMode.Shotgun:
                
                for(int i = 0; i < _numberOfShotgunLasers; i++)
                {
                    //chuncks of the totalAngleCoverage is _numberOfShotgunLasers-1
                    Quaternion laserRotation = Quaternion.AngleAxis((_totalAngleCoverage / (_numberOfShotgunLasers - 1) * i) + _startAngle, Vector3.forward);

                    lasers.Add(Instantiate(_laserPrefab, _laserSpawnTransform.position, laserRotation).GetComponent<Laser>());
                }    
                break;
        }

        foreach(var laser in lasers)
        {
            if (laser != null)
                laser.InitializeFiring(1, _disruptWeapon);
        }

        _audioSource.PlayOneShot(_laserAudioClip);
    }

    private IEnumerator ResetWeaponCooldown()
    {
        yield return new WaitForSeconds(_weaponCooldownDuration);
        _canFire = true;
    }

    //The incoming value at the moment should be 1 or -1
    // -1 losing a life
    // 1 gaining a life
    public void UpdateLives(int value)
    {
        if(value < 0 && _isShieldEnabled)
        {
            UpdateShield();
            return;
        }
        if (value < 0 && _lives > 0)
        {
            Camera.main.GetComponent<CameraBehaviour>().ShakeCamera();
            _audioSource.PlayOneShot(_playerLostLifeClip);
            _spawnManager.PlayerLostLife();
            
        }
        
        _lives = Mathf.Clamp(_lives+value, 0, _maxLives);
        _uiManager.UpdateLivesImage(_lives);

        if (_lives == 0)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            _spawnManager.Stop();
            _audioManager.GameOverAudio();
            _uiManager.DisplayGameOver();
            Instantiate(_explosionGO, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        else
        {
            //Assumed that when a life is gained the index of the engine,
            //will be two indices behind the current _lives value
            int index = value < 0 ? _lives - 1 : _lives - 2;
           _engines[index].SetActive(value < 0 ? true : false);
        }
            
            
    }

    public void EnableAcceleratedEnergyGain()
    {
        
        _currentThrusterGainRate = _thrusterAcceleratedGainRate;
        _uiManager.UpdateThrusterText(true);

        if (_thrusterAcceleratedGainCooldownRoutine != null)
            StopCoroutine(_thrusterAcceleratedGainCooldownRoutine);

        _thrusterAcceleratedGainCooldownRoutine = StartCoroutine(ResetPowerup(PowerupType.EnergyCollectible));
    }

    private void UpdateShield()
    {

        _shieldCurrentHealth--;

        switch (_shieldCurrentHealth)
        {
            case 0:
                _isShieldEnabled = false;
                _shieldGO.SetActive(false);
                break;
            case 1:
                _shieldGO.GetComponent<SpriteRenderer>().color = _shieldLowHealthColor;
                break;
            case 2:
                _shieldGO.GetComponent<SpriteRenderer>().color = _shieldMediumHealthColor;
                break;
        };
        

    }



    public void EnableShield()
    {
        _shieldCurrentHealth = _shieldMaxHealth;
        _shieldGO.GetComponent<SpriteRenderer>().color = _shieldFullHealthColor;
        _isShieldEnabled = true;
        _shieldGO.SetActive(true);
    }

    public void EnableWeapon(FiringMode mode, PowerupType type)
    {
        _firingMode = mode;
        if (_resetWeaponRountine != null)
            StopCoroutine(_resetWeaponRountine);
        switch(_firingMode)
        {
            case FiringMode.HomingMissile:
                _weaponCooldownDuration = _missileCooldownDuration;
                break;
            case FiringMode.TripleShot:
                _weaponCooldownDuration = _laserCooldownDuration;
                break;
            case FiringMode.Shotgun:
                _weaponCooldownDuration = _shotgunCooldownDuration;
                break;
        }

        _uiManager.UpdateAmmoImage((UIManager.WeaponIconName)mode);
        _resetWeaponRountine = StartCoroutine(ResetPowerup(type));
    }

    private IEnumerator ResetPowerup(PowerupType powerup)
    {
        switch(powerup)
        {
            case PowerupType.TripleShot:
            case PowerupType.HomingMissile:
            case PowerupType.WeaponDisruption:
            case PowerupType.SpeedBoost:
            case PowerupType.Shotgun:
                yield return new WaitForSeconds(_powerupCooldownDuration);
                break;
            case PowerupType.EnergyCollectible:
                yield return _thrusterAcceleratedGainCooldownWFS;
                break;
        }

        switch (powerup)
        {
            case PowerupType.TripleShot:
            case PowerupType.HomingMissile: 
                _firingMode = FiringMode.Default;
                _weaponCooldownDuration = _laserCooldownDuration;
                _uiManager.UpdateAmmoImage(UIManager.WeaponIconName.Laser);
                _audioSource.PlayOneShot(_weaponCooldownOverSound);
                break;
            case PowerupType.WeaponDisruption:
                _uiManager.UpdateDisruptionText(false);
                _disruptWeapon = false;
                break;
            case PowerupType.SpeedBoost:
                _isSpeedBoostEnabled = false;
                _thrusterGO.GetComponent<SpriteRenderer>().color = Color.white;
                break;
            case PowerupType.EnergyCollectible:
                _currentThrusterGainRate = _thrusterGainRate;
                _uiManager.UpdateThrusterText(false);
                break;
        }
    }

    public void AddAmmo()
    {
        _ammoCurrentCount = Mathf.Clamp(_ammoCurrentCount+5, 0, _ammoMaxCount);
        _ammoDelayBeforeSpawningTimer = Time.time + _ammoDelayBeforeSpawning;
        _uiManager.UpdateAmmoText(_ammoCurrentCount);
    }

    public void EnableSpeedBoost()
    {
        _isSpeedBoostEnabled = true;
        _thrusterGO.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(.15f,1f,1f);
        if (_resetSpeedBoostCoroutine != null)
            StopCoroutine(_resetSpeedBoostCoroutine);
        _resetSpeedBoostCoroutine = StartCoroutine(ResetPowerup(PowerupType.SpeedBoost));
    }

    public void AddScore(int value)
    {
        _score += value;
        bool shakeText = false;
        if (_score != 0 && _score % _healthDropScoreDivisor == 0)
        {
            _spawnManager.SpawnHealth();
            shakeText = true;
        }
            

        _uiManager.UpdateScoreText(_score, shakeText);
    }



    #endregion
}
