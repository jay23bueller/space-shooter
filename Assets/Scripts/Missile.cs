using System.Collections;
using UnityEngine;

public class Missile : Laser
{
    #region Variables
    private Transform _targetTransform;
    [SerializeField]
    private GameObject _explosionPrefab;
    [SerializeField]
    private float _maxDegreesDelta;
    private SpawnManager _spawnManager;
    [SerializeField]
    private float _movementDelay = .7f;
    private float _movementDelayTimer;
    [SerializeField]
    private float _rotationDelay;
    private bool _canRotate;
    #endregion

    #region UnityMethods

    private void Start()
    {
        _spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
    }
    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CollisionCheck(ref collision);
    }

    #endregion

    #region Methods
    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(8);
        Detonate();
    }

    private IEnumerator StartRotationRoutine()
    {
        yield return new WaitForSeconds(_rotationDelay);
        _canRotate = true;
    }

    private void Detonate()
    {
        _canMove = false;
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public override void InitializeFiring(int owner)
    {
        base.InitializeFiring(owner);
        _movementDelayTimer = Time.time + _movementDelay;
        StartCoroutine(StartRotationRoutine());
        StartCoroutine(DespawnRoutine());
    }

    protected override void Move()
    {
        if(_canMove)
        {

            if(_targetTransform != null)
            {
                Vector3 moveDirection = (_targetTransform.transform.position - transform.position).normalized;
                if (Time.time > _movementDelayTimer)
                {

                    //transform.position += (moveDirection * _speed * Time.deltaTime);
                    transform.Translate(moveDirection * _speed * Time.deltaTime, Space.World);
                }
                else
                    transform.Translate(Vector3.up * _speed * .6f * Time.deltaTime);

                if(_canRotate)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(transform.forward, moveDirection);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _maxDegreesDelta);
                }
 
            }
            else
            {
                transform.Translate(transform.up * _speed * .6f * Time.deltaTime);
                FindTarget();
            }
        }


    }

    private void FindTarget()
    {
        if (_isEnemyWeapon)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                _targetTransform = player.transform;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            _targetTransform = _spawnManager.FindNearestEnemyToPlayer();
        }
    }

    #endregion

}
