using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MovementMode
{
    WaypointVPath = 0,
    WaypointDiamondPath = 1,
    Horizontal,
    Vertical,
    ZigZag,
    Circular,
    PlayerTargeted,
    Boss
}
public class SpawnManager : MonoBehaviour
{
    #region Variables
    private bool _canSpawn = true;

    [Header("Movements")]
    public MovementInfo horizontalMovementInfo;
    public MovementInfo verticalMovementInfo;
    public MovementInfo playerTargetedMovementInfo;
    public CircularMovementInfo circularMovementInfo;
    public SelfDestructMovementInfo selfDestructMovementInfo;
    public WaypointMovementInfo diamondWaypointMovementInfo;
    public WaypointMovementInfo vWaypointMovementInfo;
    public ZigZagMovementInfo zigZagMovementInfo;
    public BossMovementInfo bossMovementInfo;

    [Header("Player")]
    [SerializeField]
    private int _ammoIncrement = 2;

    [Header("Powerups")]
    [SerializeField]
    private GameObject[] _powerups;
    [SerializeField]
    private float _minPowerupSpawnDelay = 3.0f;
    [SerializeField]
    private float _maxPowerupSpawnDelay = 7.0f;
    [SerializeField]
    private float _powerupSpawnDelayDecrement = .5f;
    [SerializeField]
    private int _turnsBeforeSpawningAmmo = 4;
    [SerializeField]
    private int _enemySpawnsBeforeAmmoDrop = 4;
    [SerializeField]
    private AudioClip _streakAndHealthClip;
    [SerializeField]
    private float _shotgunSpawningInterval = 10;
    [SerializeField]
    private int _shotgunIndex;
    [SerializeField]
    private int _healthCollectibleIndex;
    [SerializeField]
    private int _ammoCollectibleIndex;
    private int _powerupSpawnCount = 1;
    [SerializeField]
    private int _energyCollectibleIndex;
    [SerializeField]
    private WeightedIndex[] _weightedIndices;
    private Coroutine _spawnPowerupCoroutine;
    private Coroutine _spawnShotgunCoroutine;

    [Header("Enemies")]
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private List<GameObject> _enemies = new List<GameObject>();
    private Coroutine _spawnEnemyCoroutine;




    [Header("Waves")]
    [SerializeField]
    private WaveInfo[] _waves;
    [SerializeField]
    private float _delayInBetweenWaves = 2.5f;
    private int _enemiesKilled;
    private int _currentWaveIndex;
    private int _currentWaveEnemyIndex;
    private bool _spawnedAllEnemiesInWave;

    [SerializeField]
    private UIManager _uiManager;
    private Player _player;

    [Header("Energy Colletible Streak")]
    [SerializeField]
    private int _streakAmountToGetEnergyCollectible = 6;
    private int _streak;
    private float _chanceToSpawnShieldEnemyPercentage = 0f;
    [SerializeField]
    private float _spawnShieldEnemyPercentIncrement = .05f;

    private bool _waveStarted;
    public bool waveStarted { get => _waveStarted; }

    [SerializeField]
    ShaderVariantCollection _variantCollection;
    [SerializeField]
    private AudioManager _audioManager;

    #endregion
    #region UnityMethods
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        _variantCollection.WarmUp();
        
    }
    #endregion

    #region Methods
    public SelfDestructMovement CreateSelfDestructMovement()
    {
        return null;
    }
    public void PlayerLostLife()
    {
        if(_streak != 0)
        {
            _streak = 0;
            _chanceToSpawnShieldEnemyPercentage = 0f;
            _player.UpdateThrusterDrainRate(_streak);
            _uiManager.UpdateStreakText(_streak, true);
        }

    }

    public void AddEnemy(GameObject enemy)
    {
        if (enemy != null) 
        { 
            _enemies.Add(enemy);
        };
    
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (_currentWaveEnemyIndex < _waves[_currentWaveIndex].waveItems.Length)
        {
            Vector3 spawnLocation = Vector3.zero;
            WaveInfo.WaveItem waveItem = _waves[_currentWaveIndex].waveItems[_currentWaveEnemyIndex];
            //    _uiManager.EnableBossUI();
            //First enemy wave won't be delayed
            if (_currentWaveEnemyIndex != 0)
                yield return new WaitForSeconds(UnityEngine.Random.Range(waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].spawnDelays.minSpawnDelay, waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].spawnDelays.maxSpawnDelay));


            
                //Spawning number of enemies specified by the WaveInfo
                //Use the delayInBetween to delay spawning instances of current enemy
            for(int i = 0; i < waveItem.enemyWaveInfo.numberOfEnemies; i++)
            {
                bool isMirrored = false;
                switch (waveItem.waveItemMovementMode)
                {
                    case WaveInfo.WaveItemMovementMode.random:
                        isMirrored = UnityEngine.Random.value > 0.5f ? true : false;
                        break;
                    case WaveInfo.WaveItemMovementMode.mirrored:
                        isMirrored = true;
                        break;
                }
                switch (waveItem.enemyWaveInfo.enemy.movementType)
                {
                    case MovementMode.ZigZag:
                    case MovementMode.Vertical:
                    case MovementMode.WaypointDiamondPath:
                    case MovementMode.WaypointVPath:
                    case MovementMode.PlayerTargeted:
                        spawnLocation =
                            new Vector3(
                            UnityEngine.Random.Range(GameManager.LEFT_BOUND + GameManager.SPAWN_LEFTRIGHT_OFFSET, GameManager.RIGHT_BOUND - GameManager.SPAWN_LEFTRIGHT_OFFSET),
                            GameManager.ENVIRONMENT_TOP_BOUND
                            );
                        break;
                    case MovementMode.Horizontal:
                        spawnLocation = new Vector3(
                            isMirrored ? GameManager.RIGHT_BOUND : GameManager.LEFT_BOUND,
                            UnityEngine.Random.Range(GameManager.ENVIRONMENT_TOP_BOUND * .5f, GameManager.ENVIRONMENT_TOP_BOUND * .8f)
                            );
                        break;
                    case MovementMode.Circular:
                        spawnLocation =
                            new Vector3(
                                isMirrored ? GameManager.RIGHT_BOUND : GameManager.LEFT_BOUND,
                                GameManager.ENVIRONMENT_TOP_BOUND
                                );
                        break;
                    case MovementMode.Boss:
                        spawnLocation =
                            new Vector3(
                                (GameManager.RIGHT_BOUND - GameManager.LEFT_BOUND) * .5f + GameManager.LEFT_BOUND,
                                GameManager.ENVIRONMENT_TOP_BOUND);
                        break;
                }


                if (_enemyContainer != null)
                {

                    GameObject enemy = Instantiate(
                    waveItem.enemyWaveInfo.enemy.enemyType,
                    spawnLocation,
                    Quaternion.AngleAxis(180, Vector3.forward),
                    _enemyContainer.transform);

                    if (enemy != null)
                    {
                        bool enableShield = _chanceToSpawnShieldEnemyPercentage >= UnityEngine.Random.value ? true : false;

                        Movement movement = null;
                        switch(waveItem.enemyWaveInfo.enemy.movementType)
                        {
                            case MovementMode.PlayerTargeted:

                                movement = new DirectionMovement(
                                    playerTargetedMovementInfo.moveSpeed,
                                    Vector3.zero
                                    );

                                break;
                               
                            case MovementMode.Circular:

                                Vector3 diagonalStartPosition = new Vector3(isMirrored ? GameManager.RIGHT_BOUND : GameManager.LEFT_BOUND, GameManager.ENVIRONMENT_TOP_BOUND);

                                Vector3 diagonalEndPosition = (Quaternion.AngleAxis(isMirrored ? circularMovementInfo.rightSlantedDirection : circularMovementInfo.leftSlantedDirection, Vector3.forward) * Vector3.right * circularMovementInfo.distanceFromSlant) + diagonalStartPosition;

                                Vector3 slantedDirection = (diagonalEndPosition - diagonalStartPosition).normalized;
                                Vector3 radiusEndPosition = diagonalEndPosition + ((isMirrored ? Vector3.left : Vector3.right) * circularMovementInfo.radius);

                                float circularRotationSpeed = (isMirrored ? -1f : 1f) * circularMovementInfo.circularRotationSpeed;
                                float circularRadian = isMirrored ? 180f * Mathf.Deg2Rad : 0f;
                                movement = new CircularMovement(
                                    circularMovementInfo.moveSpeed,
                                    circularMovementInfo.distanceFromTargetPosition,
                                    diagonalEndPosition,
                                    slantedDirection,
                                    radiusEndPosition,
                                    circularRotationSpeed,
                                    circularMovementInfo.radius,
                                    isMirrored ? ReversibleMovement.ReversibleState.Reverse : ReversibleMovement.ReversibleState.Normal
                                    );
                                break;

                            case MovementMode.ZigZag:
                                
                                movement = new ZigZagMovement(
                                    zigZagMovementInfo.moveSpeed,
                                    isMirrored ? ReversibleMovement.ReversibleState.Reverse :
                                    ReversibleMovement.ReversibleState.Normal,
                                    zigZagMovementInfo.zigZagMaxDistance,
                                    spawnLocation.x
                                    );

                                break;

                            case MovementMode.Horizontal:

                                movement = new HorizontalMovement(
                                    horizontalMovementInfo.moveSpeed,
                                    isMirrored ? ReversibleMovement.ReversibleState.Reverse : ReversibleMovement.ReversibleState.Normal
                                    );

                                break;

                            case MovementMode.Vertical:

                                movement = new VerticalMovement(verticalMovementInfo.moveSpeed);

                                break;

                            case MovementMode.WaypointVPath:
                                movement = new WaypointMovement(
                                    vWaypointMovementInfo.moveSpeed,
                                    isMirrored ? ReversibleMovement.ReversibleState.Reverse :
                                    ReversibleMovement.ReversibleState.Normal,
                                    vWaypointMovementInfo.distanceFromNextPoint,
                                    vWaypointMovementInfo.waypoints
                                    );

                                break;
                            case MovementMode.WaypointDiamondPath:
                                movement = new WaypointMovement(
                                    diamondWaypointMovementInfo.moveSpeed,
                                    isMirrored ? ReversibleMovement.ReversibleState.Reverse :
                                    ReversibleMovement.ReversibleState.Normal,
                                    diamondWaypointMovementInfo.distanceFromNextPoint,
                                    diamondWaypointMovementInfo.waypoints
                                    );
                                break;
                            case MovementMode.Boss:
                                movement = new BossMovement(
                                    bossMovementInfo.moveSpeed,
                                    bossMovementInfo.bossInitializingSpeedMultiplier
                                    );
                            break;
                        }
                        BaseEnemy enemyComponent = enemy.GetComponent<BaseEnemy>();
                        
                        if(enemyComponent is MinionEnemy)
                        {
                            ((MinionEnemy)enemyComponent).InitializeEnemy(movement, waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].weaponFireRateDelays.minFireRateDelay,
                            waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].weaponFireRateDelays.maxFireRateDelay,
                            enableShield);
                        }
                        else
                        {
                            enemyComponent.InitializeEnemy(movement, waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].weaponFireRateDelays.minFireRateDelay,
                            waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].weaponFireRateDelays.maxFireRateDelay);
                        }


                        //enemy.GetComponent<Enemy>().SetMovementModeAndFiringDelays(
                        //    waveItem.enemyWaveInfo.enemy.movementType, isMirrored,
                        //    waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].weaponFireRateDelays.minFireRateDelay,
                        //    waveItem.enemyWaveInfo.enemy.delaysPerWave[_currentWaveIndex].weaponFireRateDelays.maxFireRateDelay,
                        //    enableShield);
                        //_enemies.Add(enemy);
                        _enemies.Add(enemy);
                    }
                    



                }

                yield return new WaitForSeconds(waveItem.enemyWaveInfo.delayInbetween);
                    
            }

            _currentWaveEnemyIndex++;

            
        }
        _spawnedAllEnemiesInWave = true;
    }

    private IEnumerator SpawnPowerupRoutine()
    {
        while(_canSpawn)
        {
            int powerupIndex;
            if (_powerupSpawnCount % _turnsBeforeSpawningAmmo == 0)
                powerupIndex = _ammoCollectibleIndex;
            else
                powerupIndex = GetWeightedRandomIndex();


            yield return new WaitForSeconds(UnityEngine.Random.Range(_minPowerupSpawnDelay, _maxPowerupSpawnDelay));


            SpawnPowerup(powerupIndex, false, Vector3.zero);
        }
    }

    private void powerupCheckForPlayerMagnet(ref Powerup powerup)
    {
        if (_player != null && _player.playerMagnetState == Player.MagnetState.Using && powerup != null)
        {
            powerup.targetTransform = _player.transform;
        }
    }

    private IEnumerator SpawnShotgunRoutine()
    {
        while (_canSpawn)
        {
            yield return new WaitForSeconds(_shotgunSpawningInterval);

            SpawnPowerup(_shotgunIndex, false, Vector3.zero);
        }
    }

    public void SpawnPowerup(int index, bool customPosition, Vector3 position)
    {
        Vector3 spawnLocation = Vector3.zero;
        if(customPosition)
        {
            spawnLocation = position;
        }
        else
        {
            if (index == _healthCollectibleIndex || index == _ammoCollectibleIndex || index == _energyCollectibleIndex)
                spawnLocation =
                        new Vector3(
                            GameManager.RIGHT_BOUND - ((GameManager.RIGHT_BOUND - GameManager.LEFT_BOUND) * .5f),
                        GameManager.ENVIRONMENT_TOP_BOUND
                        );
            else
                spawnLocation =
                        new Vector3(
                        UnityEngine.Random.Range(GameManager.LEFT_BOUND + GameManager.SPAWN_LEFTRIGHT_OFFSET, GameManager.RIGHT_BOUND - GameManager.SPAWN_LEFTRIGHT_OFFSET),
                        GameManager.ENVIRONMENT_TOP_BOUND
                        );
        }



        GameObject powerupGO = Instantiate(
            _powerups[index],
            spawnLocation,
            Quaternion.identity
            );

        if (powerupGO != null)
        {
            Powerup powerup = powerupGO.GetComponent<Powerup>();
            powerupCheckForPlayerMagnet(ref powerup);
        }
        _powerupSpawnCount++;

    }
   
    public void SpawnHealth()
    {
        SpawnPowerup(_healthCollectibleIndex, false, Vector3.zero);
        AudioSource.PlayClipAtPoint(_streakAndHealthClip, Camera.main.transform.position);
    }

    public void SpawnAmmoCollectible()
    {
        SpawnPowerup(_ammoCollectibleIndex, false, Vector3.zero);

        AudioSource.PlayClipAtPoint(_streakAndHealthClip, Camera.main.transform.position);
    }

    public void Stop()
    {
        StopAllCoroutines();
        _enemies.Clear();
        Destroy(_enemyContainer);

    }

    public void StartWave(float delay)
    {
        _player.UpdateAmmoCapacityAndResetCurrentAmmo(_ammoIncrement * _currentWaveIndex);
        if(_currentWaveIndex > 0)
        {
            _minPowerupSpawnDelay -= _powerupSpawnDelayDecrement;
            _maxPowerupSpawnDelay -= _powerupSpawnDelayDecrement;
            _shotgunSpawningInterval -= _powerupSpawnDelayDecrement;
        }
        _player.UpdatePowerupDuration(_currentWaveIndex);
        _uiManager.UpdateWaveText(_currentWaveIndex + 1);
        _uiManager.DisplayWaveText(true);
        if (_waves[_currentWaveIndex].isBossWave)
            _audioManager.StartBossMusic();
        StartCoroutine(StartWaveRoutine(delay));
    }

    public void EnemyDestroyed(GameObject enemy, float powerupSpawnDelayDuration, bool wasKilled, bool isBoss)
    {
        if(_canSpawn)
        {
            _enemies.Remove(enemy);
            bool shakeStreakText = false;
            Vector3 position = enemy.transform.position;

            if (wasKilled)
            {
                _streak++;
                _enemiesKilled++;

                if (_enemiesKilled % _enemySpawnsBeforeAmmoDrop == 0)
                {
                    StartCoroutine(SpawnPowerupAtPositionRoutine(position, powerupSpawnDelayDuration, _ammoCollectibleIndex));
                }

                if (_streak != 0 && _streak % _streakAmountToGetEnergyCollectible == 0)
                {
                    shakeStreakText = true;
                    int currentStreakLevel = (_streak / _streakAmountToGetEnergyCollectible);
                    if (_player != null)
                    {
                        _player.UpdateThrusterDrainRate(currentStreakLevel);
                    }
                  
                    _chanceToSpawnShieldEnemyPercentage = currentStreakLevel * _spawnShieldEnemyPercentIncrement;
                    StartCoroutine(SpawnPowerupAtPositionRoutine(new Vector3(GameManager.RIGHT_BOUND - (GameManager.RIGHT_BOUND - GameManager.LEFT_BOUND) * .5f, GameManager.ENVIRONMENT_TOP_BOUND), powerupSpawnDelayDuration, _energyCollectibleIndex));
                    AudioSource.PlayClipAtPoint(_streakAndHealthClip, Camera.main.transform.position);
                }
            }
                


            _uiManager.UpdateStreakText(_streak, shakeStreakText);
            if (_spawnedAllEnemiesInWave && _enemies.Count <= 0)
            {
                if(isBoss)
                {
                    if(_player != null) { _player.GetComponent<Collider2D>().enabled = false; }
                }
                StopCoroutine(_spawnEnemyCoroutine);
                StopCoroutine(_spawnPowerupCoroutine);
                StopCoroutine(_spawnShotgunCoroutine);

                _currentWaveEnemyIndex = 0;
                _spawnedAllEnemiesInWave = false;
                _waveStarted = false;
                _currentWaveIndex++;
                if (_currentWaveIndex < _waves.Length)
                {
                    StartWave(_delayInBetweenWaves);
                }
                else
                {
                    _uiManager.DisplayWinText();
                    _audioManager.StartWinMusic();
                }
            }
        }
        
    }

    private IEnumerator SpawnPowerupAtPositionRoutine(Vector3 position, float delay, int index)
    {
        yield return new WaitForSeconds(delay);
        SpawnPowerup(index, true, position);
    }

    private IEnumerator StartWaveRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _waveStarted = true;
        _uiManager.DisplayWaveText(false);
        _spawnEnemyCoroutine = StartCoroutine(SpawnEnemyRoutine());
        _spawnPowerupCoroutine = StartCoroutine(SpawnPowerupRoutine());
        _spawnShotgunCoroutine = StartCoroutine(SpawnShotgunRoutine());
    }

    public Transform FindNearestEnemyToPlayer()
    {
        Transform closestEnemyTransform = null;
        
        float distance = -1f;
        foreach (GameObject enemy in _enemies)
        {
            if (_player == null)
                return null;

            if (distance == -1 || closestEnemyTransform == null)
            {
                closestEnemyTransform = enemy != null && !enemy.GetComponent<BaseEnemy>().isDying ? enemy.transform : null;

                if (closestEnemyTransform != null)
                    distance = Vector3.Distance(_player.transform.position, closestEnemyTransform.position);
            }
            else if (enemy != null)
            {
                float newEnemyDistance = Vector3.Distance(_player.transform.position, enemy.transform.position);
                if (newEnemyDistance < distance)
                {
                    closestEnemyTransform = enemy != null && !enemy.GetComponent<BaseEnemy>().isDying ? enemy.transform : closestEnemyTransform;
                    if (enemy != null)
                        distance = newEnemyDistance;
                }
            }
        }

        return closestEnemyTransform;

    }

    private int GetWeightedRandomIndex()
    {
        float totalWeight = _weightedIndices.Sum(currentWeightedIndex => currentWeightedIndex.weight);

        float scaledRandomValue = UnityEngine.Random.value * totalWeight;
        float currentTotal = 0f;

        for(int i = 0; i < _weightedIndices.Length; i++)
        {
            currentTotal += _weightedIndices[i].weight / totalWeight;
           
            if (currentTotal >= scaledRandomValue)
            {
                return _weightedIndices[i].index;
            }
                
        }

        return -1;
    }

    [Serializable]
    private struct WeightedIndex
    {
        public float weight;
        public int index;
    }
    #endregion
}
