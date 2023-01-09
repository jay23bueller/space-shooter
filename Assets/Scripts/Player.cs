using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum FiringMode
{
    Default,
    HomingMissile,
    TripleShot
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
    private float _thrusterDrainRate = -2.5f;
    [SerializeField]
    private float _thrusterGainRate = 2.5f;
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
    private bool _punishPlayer;
    private bool _fullyCharged;
    private bool _justPunished;
    private bool _playBoostSound = true;
    private bool _engagingThrusters;
    private float _delayThrusterDisabledSoundTimer;
    private float _thrusterMaxEnergy = 100f;
    private float _thrusterMinEnergy = 0f;

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

    //Laser
    [Header("Weapon/Laser")]
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private Transform _laserSpawnTransform;
    [SerializeField]
    private float _laserCooldownDuration = .2f;
    [SerializeField]
    private AudioClip _laserAudioClip;

    //Missile
    [Header("Weapon/Laser")]
    [SerializeField]
    private GameObject _missilePrefab;
    [SerializeField]
    private AudioClip _missileAudioClip;
    [SerializeField]
    private float _missileCooldownDuration = 1f;
    private float _weaponCooldownDuration;


    //Shield
    [Header("Shield")]
    [SerializeField]
    private GameObject _shieldGO;
    private int _shieldMaxHealth = 3;
    private int _shieldCurrentHealth;
    private bool _isShieldEnabled;

    //Managers
    [Header("Manager Info")]
    [SerializeField]
    private UIManager _uiManager;
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
    private AudioSource _audioSource;


    //Lives
    [Header("Lives")]
    [SerializeField]
    private int _lives = 3;
    [SerializeField]
    private AudioClip _playerLostLifeClip;
    private int _maxLives = 3;

    private bool _disruptWeapon;

    #endregion

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {
        //Set starting position
        transform.position = new Vector3(0f,0f,0f);
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWN_MANAGER_TAG).GetComponent<SpawnManager>();
        _ammoCurrentCount = _ammoMaxCount;
        _uiManager.UpdateScoreText(_score);
        _uiManager.UpdateAmmoText(_ammoCurrentCount);
        _uiManager.SetAmmoMaxCount(_ammoMaxCount);
        _anim = GetComponent<Animator>();
        if (_spawnManager == null)
            Debug.LogError("The Spawn Manager is NULL");

        if (_uiManager == null)
            Debug.LogError("The UI Manager is NULL");

        _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
            Debug.LogError("Player missing AudioSource component!");

        _thrusterCurrentEnergy = _thrusterMaxEnergy;
    }

    // Update is called once per frame
    void Update()
    {
        CheckThrusterPower();
        CheckForThrusterInput();
        MoveCharacter();
        UpdateThrusterUI();
        FireWeapon();

    }


    #endregion

    #region Methods

    private void CheckForThrusterInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (!_punishPlayer)
            {
                _engagingThrusters = true;

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
            
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _engagingThrusters = false;
            _playBoostSound = true;
            
        }
            

    }

    public void EnableWeaponDistruption()
    {
        if(!_disruptWeapon)
        {
            _disruptWeapon = true;
            _uiManager.UpdateDisruptionText(true);
            StartCoroutine(WeaponDisruptionResetRoutine());
        }
            
    }

    private IEnumerator WeaponDisruptionResetRoutine()
    {
        yield return new WaitForSeconds(5);
        _uiManager.UpdateDisruptionText(false);
        _disruptWeapon = false;
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
            StartCoroutine(PunishPlayerRoutine());
        }
        if(!_fullyCharged && !_punishPlayer)
        {

            float batteryCharge = Time.deltaTime * _thrusterGainRate * ((_thrusterCurrentEnergy < .5f * _thrusterMaxEnergy) && _justPunished ? _thrusterRecoveryMultiplier : 1f);

            _thrusterCurrentEnergy = Mathf.Clamp(batteryCharge + _thrusterCurrentEnergy, _thrusterMinEnergy, _thrusterMaxEnergy);
            if (_thrusterCurrentEnergy >= .5f * _thrusterMaxEnergy)
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
            _thrusterCurrentEnergy = Mathf.Clamp(_thrusterCurrentEnergy + (_thrusterDrainRate * Time.deltaTime), _thrusterMinEnergy, _thrusterMaxEnergy);
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

    //Attempt to fire weapon
    private void FireWeapon()
    {
        if(_canFire && Input.GetKeyDown(KeyCode.Space))
        {
            if(_ammoCurrentCount > 0)
            {
                _ammoCurrentCount--;
                _uiManager.UpdateAmmoText(_ammoCurrentCount);

                switch (_firingMode)
                {
                    case FiringMode.Default:
                    case FiringMode.TripleShot:
                        List<Laser> lasers = new List<Laser>();
                        if (FiringMode.Default == _firingMode)
                            lasers.Add(Instantiate(_laserPrefab, _laserSpawnTransform.position, _laserSpawnTransform.rotation).GetComponent<Laser>());
                        else
                            lasers.AddRange(Instantiate(_tripleShotPrefab, _laserSpawnTransform.position, _laserSpawnTransform.rotation).GetComponentsInChildren<Laser>());
                        
                        foreach (Laser laser in lasers)
                            laser.InitializeFiring(1, _disruptWeapon);
                        
                        _audioSource.PlayOneShot(_laserAudioClip);
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
        if (value < 0)
        {
            Camera.main.GetComponent<CameraBehaviour>().ShakeCamera();
            _audioSource.PlayOneShot(_playerLostLifeClip);
        }
        
        _lives = Mathf.Clamp(_lives+value, 0, _maxLives);
        _uiManager.UpdateLivesImage(_lives);

        if (_lives == 0)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            _spawnManager.Stop();
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

    private void UpdateShield()
    {

        _shieldCurrentHealth--;

        switch (_shieldCurrentHealth)
        {
            case 0:
                _isShieldEnabled = false;
                _shieldGO.SetActive(false);
                _shieldGO.GetComponent<SpriteRenderer>().color = Color.white;
                break;
            case 1:
                _shieldGO.GetComponent<SpriteRenderer>().color = Color.red;
                break;
            case 2:
                _shieldGO.GetComponent<SpriteRenderer>().color = Color.yellow;
                break;
        };
        

    }

    public void EnableShield()
    {
        _shieldCurrentHealth = _shieldMaxHealth;
        _shieldGO.GetComponent<SpriteRenderer>().color = Color.white;
        _isShieldEnabled = true;
        _shieldGO.SetActive(true);
    }

    public void EnableWeapon(FiringMode mode)
    {
        _firingMode = mode;
        if (_resetWeaponRountine != null)
            StopCoroutine(_resetWeaponRountine);
        PowerupType powerup = _firingMode == FiringMode.TripleShot ? PowerupType.TripleShot : PowerupType.HomingMissile;
        _weaponCooldownDuration = mode == FiringMode.HomingMissile ? _missileCooldownDuration : _laserCooldownDuration;
        _resetWeaponRountine = StartCoroutine(ResetPowerup(powerup));
    }

    private IEnumerator ResetPowerup(PowerupType powerup)
    {
        yield return new WaitForSeconds(5.0f);

        switch (powerup)
        {
            case PowerupType.TripleShot:
            case PowerupType.HomingMissile: 
                _firingMode = FiringMode.Default;
                _weaponCooldownDuration = _laserCooldownDuration;
                break;
            case PowerupType.SpeedBoost:
                _isSpeedBoostEnabled = false;
                break;
        }
    }

    public void AddAmmo()
    {
        _ammoCurrentCount = Mathf.Clamp(_ammoCurrentCount+5, 0, _ammoMaxCount);
        _uiManager.UpdateAmmoText(_ammoCurrentCount);
    }

    public void EnableSpeedBoost()
    {
        _isSpeedBoostEnabled = true;
        if (_resetSpeedBoostCoroutine != null)
            StopCoroutine(_resetSpeedBoostCoroutine);
        _resetSpeedBoostCoroutine = StartCoroutine(ResetPowerup(PowerupType.SpeedBoost));
    }

    public void AddScore(int value)
    {
        _score += value;
        if (_score % _healthDropScoreDivisor == 0)
            _spawnManager.SpawnHealth();

        _uiManager.UpdateScoreText(_score);
    }



    #endregion
}
