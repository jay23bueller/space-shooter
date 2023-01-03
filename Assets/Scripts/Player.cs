using System.Collections;
using System.Collections.Generic;
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
    private const float LEFT_BOUND = -0.01f;
    private const float RIGHT_BOUND = 1.02f;
    private const float TOP_BOUND = 0.5f;
    private const float BOTTOM_BOUND = 0.05f;
    private const string SPAWN_MANAGER_TAG = "SpawnManager";
    #endregion

    #region Variables

    //Movement
    [SerializeField]
    private float _speed = 3.5f;
    [SerializeField]
    private float _speedMultiplier = 1.0f;
    private float _defaultSpeedMultiplier = 1.0f;


    //Thruster
    [AddComponentMenu("Thrusters")]
    [SerializeField]
    private float _thrusterBoostMultiplier = 1.3f;
    private bool _engagingThrusters;
    [SerializeField]
    private GameObject _thrusterGO;
    [SerializeField]
    private float _thrusterDrainRate = -2.5f;
    [SerializeField]
    private float _thrusterGainRate = 2.5f;
    private float _thrusterMaxPower = 100f;
    private float _thrusterMinPower = 0f;
    [SerializeField]
    private float _thrusterCurrentPower;
    private bool _punishPlayer;
    private bool _fullyCharged;
    private bool _justPunished;
    [SerializeField]
    private float _thrusterRecoveryMultiplier = 2.0f;
    [SerializeField]
    private AudioClip _outOfThrusterEnergyClip;
    private float _delayThrusterDisabledSoundTimer;
    [SerializeField]
    private float _thrusterDisabledSoundInterval = .3f;

    //SpeedBoost
    private Coroutine _resetSpeedBoostCoroutine;
    private bool _isSpeedBoostEnabled;
    [SerializeField]
    private float _speedBoostMultipler = 2.0f;

    //TripleShot
    [SerializeField]
    private GameObject _tripleShotPrefab;
    private Coroutine _resetWeaponRountine;


    //Current Weapon
    private bool _canFire = true;
    [SerializeField]
    private int _ammoMaxCount = 15;
    private int _ammoCurrentCount = 15;
    [SerializeField]
    private AudioClip _outOfAmmoClip;
    private FiringMode _firingMode;

    //Laser
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private Transform _laserSpawnTransform;
    [SerializeField]
    private float _laserCooldownDuration = .2f;
    [SerializeField]
    private AudioClip _laserAudioClip;

    //Missile
    [SerializeField]
    private GameObject _missilePrefab;
    private float _weaponCooldownDuration;
    [SerializeField]
    private AudioClip _missileAudioClip;
    [SerializeField]
    private float _missileCooldownDuration = 1f;


    private float _initialViewportZPosition;

    //Shield
    private bool _isShieldEnabled;
    [SerializeField]
    private GameObject _shieldGO;
    private int _shieldMaxHealth = 3;
    private int _shieldCurrentHealth;

    //Managers
    private SpawnManager _spawnManager;
    [SerializeField]
    private UIManager _uiManager;

    //Score
    [SerializeField]
    private int _healthDropScoreDivisor = 200;
    private int _score;

    //Effects
    [SerializeField]
    private GameObject[] _engines;
    private AudioSource _audioSource;

    [SerializeField]
    private GameObject _explosionGO;

    //Lives
    [SerializeField]
    private int _lives = 3;
    private int _maxLives = 3;
    #endregion

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {
        //Set starting position
        transform.position = new Vector3(0f,0f,0f);
        _initialViewportZPosition = Camera.main.WorldToViewportPoint(transform.position).z;
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWN_MANAGER_TAG).GetComponent<SpawnManager>();
        _ammoCurrentCount = _ammoMaxCount;
        _uiManager.UpdateScoreText(_score);
        _uiManager.UpdateAmmoText(_ammoCurrentCount);

        if (_spawnManager == null)
            Debug.LogError("The Spawn Manager is NULL");

        if (_uiManager == null)
            Debug.LogError("The UI Manager is NULL");

        _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
            Debug.LogError("Player missing AudioSource component!");

        _thrusterCurrentPower = _thrusterMaxPower;
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
                _engagingThrusters = true;
            else if(Time.time > _delayThrusterDisabledSoundTimer)
            {
                _delayThrusterDisabledSoundTimer = _thrusterDisabledSoundInterval + Time.time;
                _audioSource.PlayOneShot(_outOfThrusterEnergyClip);
            }
                
        }
            
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _engagingThrusters = false;
        }
            

    }

    private void UpdateThrusterUI()
    {
        _uiManager.UpdateThrusterSlider(_thrusterCurrentPower, _justPunished);
    }

    private IEnumerator PunishPlayerRoutine()
    {
        _punishPlayer = true;
        yield return new WaitForSeconds(2f);
        _punishPlayer = false;

    }

    private void CheckThrusterPower()
    {
        if(_thrusterCurrentPower == _thrusterMinPower && !_punishPlayer && !_justPunished)
        {
            _justPunished = true;
            StartCoroutine(PunishPlayerRoutine());
        }
        if(!_fullyCharged && !_punishPlayer)
        {

            float batteryCharge = Time.deltaTime * _thrusterGainRate * ((_thrusterCurrentPower < .5f * _thrusterMaxPower) && _justPunished ? _thrusterRecoveryMultiplier : 1f);

            _thrusterCurrentPower = Mathf.Clamp(batteryCharge + _thrusterCurrentPower, _thrusterMinPower, _thrusterMaxPower);
            if (_thrusterCurrentPower >= .5f * _thrusterMaxPower)
            {
                _justPunished = false;
            }
            if (_thrusterCurrentPower == _thrusterMaxPower)
                _fullyCharged = true;
        }
    }

    private void SetBoost()
    {
        if (_isSpeedBoostEnabled)
        {
            _speedMultiplier = _speedBoostMultipler;
        }
        else if (_engagingThrusters && !_justPunished)
        {
            _speedMultiplier = _thrusterBoostMultiplier;
            _thrusterCurrentPower = Mathf.Clamp(_thrusterCurrentPower +(_thrusterDrainRate * Time.deltaTime), _thrusterMinPower, _thrusterMaxPower);
            _fullyCharged = false;
            Debug.Log(_thrusterCurrentPower);
        }
        else
        {
            _speedMultiplier = _defaultSpeedMultiplier;
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

        Vector2 nextVerticalViewportPosition = Camera.main.WorldToViewportPoint(transform.position + verticalDirection);
        Vector2 nextHorizontalViewportPosition = Camera.main.WorldToViewportPoint(transform.position + verticalDirection);

        //If the character's new position is within the top and bottom bounds, then move it
        if (nextVerticalViewportPosition.y < TOP_BOUND && nextVerticalViewportPosition.y > BOTTOM_BOUND)
            transform.Translate(verticalDirection);

        //If the character's new position is outside the left or right bounds, teleport the character
        if (nextHorizontalViewportPosition.x > RIGHT_BOUND)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0f, nextHorizontalViewportPosition.y, _initialViewportZPosition));
        }
        else if (nextHorizontalViewportPosition.x < LEFT_BOUND)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(1f, nextHorizontalViewportPosition.y, _initialViewportZPosition));
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
                            laser.InitializeFiring(1);
                        
                        _audioSource.PlayOneShot(_laserAudioClip);
                        break;

                    case FiringMode.HomingMissile:
                        Missile missile = Instantiate(_missilePrefab, _laserSpawnTransform.position, _laserSpawnTransform.rotation).GetComponent<Missile>();
                        missile.InitializeFiring(1);
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
