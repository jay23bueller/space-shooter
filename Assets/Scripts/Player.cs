using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField]
    private float _speed = 3.5f;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private Transform _laserSpawnTransform;
    private bool _canFire = true;
    [SerializeField]
    private float _laserCooldownDuration = .2f;
    [SerializeField]
    private int _lives = 3;
    private SpawnManager _spawnManager;
    private float _initialViewportZPosition;
    private bool _isTripleShotEnabled;
    [SerializeField]
    private GameObject _tripleShotPrefab;
    private Coroutine _resetTripleShotCoroutine;
    #endregion

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {
        //Set starting position
        transform.position = new Vector3(0f,0f,0f);
        _initialViewportZPosition = Camera.main.WorldToViewportPoint(transform.position).z;
        _spawnManager = GameObject.FindGameObjectWithTag(SPAWN_MANAGER_TAG).GetComponent<SpawnManager>();

        if (_spawnManager == null)
            Debug.LogError("The Spawn Manager is NULL");
    }

    // Update is called once per frame
    void Update()
    {
        MoveCharacter();
        FireLaser();
    }

    #endregion

    #region Methods

    //Move the character based on input within the viewport
    private void MoveCharacter()
    {

        Vector3 verticalDirection = Vector3.up * Input.GetAxis(VERTICAL_AXIS) * _speed * Time.deltaTime;
        Vector3 horizontalDirection = Vector3.right * Input.GetAxis(HORIZONTAL_AXIS) * _speed * Time.deltaTime;

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
            if (_isTripleShotEnabled)
                Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity);
            else
                Instantiate(_laserPrefab, _laserSpawnTransform.position, _laserSpawnTransform.rotation);
            _canFire = false;
            StartCoroutine(ResetLaserCooldown());
        }
            
    }

    private IEnumerator ResetLaserCooldown()
    {
        yield return new WaitForSeconds(_laserCooldownDuration);
        _canFire = true;
    }

    public void TakeDamage()
    {
        _lives--;

        if (_lives <= 0)
        {
            _spawnManager.Stop();
            Destroy(gameObject);
        }
            
    }

    public void EnableTripleShot()
    {
        _isTripleShotEnabled = true;
        if (_resetTripleShotCoroutine != null)
            StopCoroutine(_resetTripleShotCoroutine);
        _resetTripleShotCoroutine = StartCoroutine(ResetTripleShotRoutine());
    }

    private IEnumerator ResetTripleShotRoutine()
    {
        yield return new WaitForSeconds(5f);
        _isTripleShotEnabled = false;
    }

    #endregion
}
