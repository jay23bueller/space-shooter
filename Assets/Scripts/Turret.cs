using UnityEngine;

public class Turret : MonoBehaviour
{
    #region Variables
    private bool _canFire;
    private Transform _playerTransform;
    [SerializeField]
    private float _firingDelay = 1f;
    [SerializeField]
    private GameObject _laserPrefab;
    private float _firingTimer;
    [SerializeField]
    private AudioClip _laserAudioClip;
    private AudioSource _audioSource;
    #endregion

    #region UnityMethods
    private void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        _audioSource = GetComponent<AudioSource>();
        if(playerGO != null) { _playerTransform = playerGO.transform; }
        _firingTimer = Time.time + _firingDelay;
    }

    private void Update()
    {
        Rotate();
        if(_canFire)
            FireLaser();
    }
    #endregion

    #region Methods

    private void FireLaser()
    {
        if(_firingTimer < Time.time)
        {
            if(_playerTransform != null)
            {
                Quaternion enemyToPlayerRotation = Quaternion.LookRotation(transform.forward, -transform.up);
                var laserGO = Instantiate(_laserPrefab, transform.position + (-transform.up * 1f), enemyToPlayerRotation);
                _audioSource.PlayOneShot(_laserAudioClip);
                if (laserGO != null) { laserGO.GetComponent<Laser>().InitializeFiring(0, false); }
                _firingTimer = Time.time + _firingDelay;
            }
        }
    }

    private void Rotate()
    {
        if(_playerTransform != null)
        {
            Vector3 lastKnownPlayerPosition = _playerTransform.position;
            Quaternion enemyToPlayerRotation = Quaternion.LookRotation(transform.forward, -(lastKnownPlayerPosition - transform.position).normalized);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, enemyToPlayerRotation, 5f);
        }
    }

    public void EnableFiring(bool canFire)
    {
        _canFire = canFire;
    }
    #endregion
}
