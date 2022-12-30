using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField]
    private float _thrusterBoostMultiplier = 1.3f;
    private bool _isThrustersEnabled;
    [SerializeField]
    private GameObject _thrusterGO;

    //SpeedBoost
    private Coroutine _resetSpeedBoostCoroutine;
    private bool _isSpeedBoostEnabled;
    [SerializeField]
    private float _speedBoostMultipler = 2.0f;

    //TripleShot
    [SerializeField]
    private GameObject _tripleShotPrefab;
    private Coroutine _resetTripleShotCoroutine;
    private bool _isTripleShotEnabled;

    //Laser
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private Transform _laserSpawnTransform;
    private bool _canFire = true;
    [SerializeField]
    private float _laserCooldownDuration = .2f;
    private int _laserCurrentCount = 15;
    [SerializeField]
    private AudioClip _laserAudioClip;
    [SerializeField]
    private AudioClip _outOfAmmoClip;
    [SerializeField]
    private int _laserMaxCount = 15;

    [SerializeField]
    private int _lives = 3;
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

    private int _score;

    //Effects
    [SerializeField]
    private GameObject[] _engines;
    private AudioSource _audioSource;

    [SerializeField]
    private GameObject _explosionGO;
    #endregion

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {
        //Set starting position
        transform.position = new Vector3(0f,0f,0f);
        _initialViewportZPosition = Camera.main.WorldToViewportPoint(transform.position).z;
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWN_MANAGER_TAG).GetComponent<SpawnManager>();
        _laserCurrentCount = _laserMaxCount;
        _uiManager.UpdateScoreText(_score);
        _uiManager.UpdateAmmoText(_laserCurrentCount);

        if (_spawnManager == null)
            Debug.LogError("The Spawn Manager is NULL");

        if (_uiManager == null)
            Debug.LogError("The UI Manager is NULL");

        _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
            Debug.LogError("Player missing AudioSource component!");
    }

    // Update is called once per frame
    void Update()
    {
        CheckForThrusterBoost();
        MoveCharacter();
        FireLaser();

    }


    #endregion

    #region Methods

    private void CheckForThrusterBoost()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            _isThrustersEnabled = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            _isThrustersEnabled = false;

    }

    //Move the character based on input within the viewport
    private void MoveCharacter()
    {
        if(_isSpeedBoostEnabled)
        {
            _speedMultiplier = _speedBoostMultipler;
        } 
        else if(_isThrustersEnabled)
        {
            _speedMultiplier = _thrusterBoostMultiplier;
        }
        else
        {
            _speedMultiplier = _defaultSpeedMultiplier;
        }

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

    //Attempt to fire a laser
    private void FireLaser()
    {
        if(_canFire && Input.GetKeyDown(KeyCode.Space))
        {
            if(_laserCurrentCount > 0)
            {
                _laserCurrentCount--;
                _uiManager.UpdateAmmoText(_laserCurrentCount);
                if (_isTripleShotEnabled)
                {
                    Laser[] lasers = Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity).GetComponentsInChildren<Laser>();
                    foreach (Laser laser in lasers)
                    {
                        laser.InitializeFiring(1);
                    }
                }

                else
                {
                    Laser laser = Instantiate(_laserPrefab, _laserSpawnTransform.position, _laserSpawnTransform.rotation).GetComponent<Laser>();
                    laser.InitializeFiring(1);
                }
                _audioSource.PlayOneShot(_laserAudioClip);
                _canFire = false;
                StartCoroutine(ResetLaserCooldown());
            }
            else
            {
                _audioSource.PlayOneShot(_outOfAmmoClip);
            }
   

        }
            
    }

    private IEnumerator ResetLaserCooldown()
    {
        yield return new WaitForSeconds(_laserCooldownDuration);
        _canFire = true;
    }

    public void TakeDamage()
    {
        if(_isShieldEnabled)
        {
            UpdateShield();
            return;
        }
        

        _lives--;
        _uiManager.UpdateLivesImage(_lives);

        if (_lives <= 0)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            _spawnManager.Stop();
            _uiManager.DisplayGameOver();
            Instantiate(_explosionGO, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        else
            _engines[_lives - 1].SetActive(true);
            
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

    public void EnableTripleShot()
    {
        _isTripleShotEnabled = true;
        if (_resetTripleShotCoroutine != null)
            StopCoroutine(_resetTripleShotCoroutine);
        _resetTripleShotCoroutine = StartCoroutine(ResetPowerup(0));
    }

    private IEnumerator ResetPowerup(int powerupID)
    {
        yield return new WaitForSeconds(5.0f);

        switch (powerupID)
        {
            case 0:
                //Triple Shot
                _isTripleShotEnabled = false;
                break;
            case 1:
                //Speed Boost
                _isSpeedBoostEnabled = false;
                break;
            default:
                Debug.LogError("Incorrect powerupID was passed!");
                break;
        }
    }

    public void AddAmmo()
    {
        _laserCurrentCount = Mathf.Clamp(_laserCurrentCount+5, 0, _laserMaxCount);
        _uiManager.UpdateAmmoText(_laserCurrentCount);
    }

    public void EnableSpeedBoost()
    {
        _isSpeedBoostEnabled = true;
        if (_resetSpeedBoostCoroutine != null)
            StopCoroutine(_resetSpeedBoostCoroutine);
        _resetSpeedBoostCoroutine = StartCoroutine(ResetPowerup(1));
    }

    public void AddScore(int value)
    {
        _score += value;
        _uiManager.UpdateScoreText(_score);
    }



    #endregion
}
