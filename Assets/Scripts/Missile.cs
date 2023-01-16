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
    private float _findTargetDelay = .6f;
    private float _findTargetDelayTimer;
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

    public override void InitializeFiring(int owner, bool disrupted)
    {
        base.InitializeFiring(owner, disrupted);
        _movementDelayTimer = Time.time + _movementDelay;
        StartCoroutine(StartRotationRoutine());
        StartCoroutine(DespawnRoutine());
    }

    protected override void Move()
    {
        if(_canMove)
        {

            if(_targetTransform != null && !_targetTransform.GetComponent<Enemy>().isDying)
            {
                Vector3 targetPosition = _targetTransform.position;
                Vector3 moveDirection = (targetPosition - transform.position).normalized;
                if (Time.time > _movementDelayTimer)
                {

                    //transform.position += (moveDirection * _speed * Time.deltaTime);
                    if(Vector3.Distance(transform.position, targetPosition) > .1f)
                        transform.Translate(moveDirection * _speed * Time.deltaTime, Space.World);
                }
                else
                    transform.Translate(Vector3.up * _speed * .6f * Time.deltaTime);

                if(_canRotate && _targetTransform != null)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(transform.forward, moveDirection);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _maxDegreesDelta);
                }
 
            }
            else
            {
                transform.Translate(transform.up * _speed * .6f * Time.deltaTime, Space.World);
                if(_findTargetDelayTimer < Time.time)
                {
                    _findTargetDelayTimer = Time.time + _findTargetDelay;
                    FindTarget();
                }
                
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
