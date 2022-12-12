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
    #endregion

    #region Variables
    [SerializeField]
    private float _speed = 3.5f;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private Transform _laserSpawnTransform;
    private bool _canFire = true;
    //private float _cooldownTimer;
    [SerializeField]
    private float _laserCooldownDuration = .2f;

    #endregion

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {
        //Set starting position
        transform.position = new Vector3(0f,0f,0f);
    }

    // Update is called once per frame
    void Update()
    {
        moveCharacter();
        fireLaser();
    }
    #endregion

    #region Methods

    //Move the character based on input within the viewport
    private void moveCharacter()
    {

        Vector3 verticalDirection = Vector3.up * Input.GetAxis(VERTICAL_AXIS) * _speed * Time.deltaTime;
        Vector3 horizontalDirection = Vector3.right * Input.GetAxis(HORIZONTAL_AXIS) * _speed * Time.deltaTime;

        Vector3 nextVerticalViewportPosition = Camera.main.WorldToViewportPoint(transform.position + verticalDirection);
        Vector3 nextHorizontalViewportPosition = Camera.main.WorldToViewportPoint(transform.position + verticalDirection);

        //If the character's new position is within the top and bottom bounds, then move it
        if (nextVerticalViewportPosition.y < TOP_BOUND && nextVerticalViewportPosition.y > BOTTOM_BOUND)
            transform.Translate(verticalDirection);

        //If the character's new position is outside the left or right bounds, teleport the character
        if (nextHorizontalViewportPosition.x > RIGHT_BOUND)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0f, nextHorizontalViewportPosition.y, nextHorizontalViewportPosition.z));
        }
        else if (nextHorizontalViewportPosition.x < LEFT_BOUND)
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(1f, nextHorizontalViewportPosition.y, nextHorizontalViewportPosition.z));
        }

        transform.Translate(horizontalDirection);

    }

    //Attempt to fire a laser
    private void fireLaser()
    {
        //One approach
        //if (_canFire && Input.GetKeyDown(KeyCode.Space))
        //{
        //    Instantiate(_laserPrefab, _laserSpawnLocationTransform.position, _laserSpawnLocationTransform.rotation);
        //    _canFire = false;
        //    _cooldownTimer = Time.time + _cooldownDuration;
        //}
        //else if(Time.time >= _cooldownTimer)
        //{
        //    _canFire = true;
        //}
        
        //Approach two
        if(_canFire && Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(_laserPrefab, _laserSpawnTransform.position, _laserSpawnTransform.rotation);
            _canFire = false;
            StartCoroutine(resetLaserCooldown());
        }
            
    }

    private IEnumerator resetLaserCooldown()
    {
        yield return new WaitForSeconds(_laserCooldownDuration);
        _canFire = true;
    }
    #endregion
}
