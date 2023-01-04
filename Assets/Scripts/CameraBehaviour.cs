using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private float _cameraShakeMagnitudeX = 6f;
    [SerializeField]
    private float _cameraShakeMagnitudeY = 5f;
    [SerializeField]
    private float _minSampleValue = 0f;
    [SerializeField]
    private float _maxSampleValue = 1000f;
    private Vector3 _startPosition;
    private bool _canShake = true;
    [SerializeField]
    private float _shakeDelayDuration = .05f;
    private WaitForSeconds _shakeDelayWFS;
    [SerializeField]
    private int _maxShakeAmount = 15;
    #endregion

    #region UnityMethods
    private void Start()
    {
        _startPosition = transform.position;
        _shakeDelayWFS = new WaitForSeconds(_shakeDelayDuration);
    }
    #endregion

    #region Methods
    public void ShakeCamera()
    {
        if (_canShake)
            StartCoroutine(ShakeCameraRoutine());
    }

    private IEnumerator ShakeCameraRoutine()
    {
        _canShake = false;
        for(int i = 0; i < _maxShakeAmount; i++)
        {
            Vector3 displacement = GetRandomDisplacementFromStartPosition();
            transform.position = displacement;
            yield return _shakeDelayWFS;
            transform.position = _startPosition;
        }
        _canShake = true;
    }

    private float GetRandomPerlinNoiseSamplePointFromSampleArea()
    {
        float randomX = Random.Range(_minSampleValue, _maxSampleValue);
        float randomY = Random.Range(_minSampleValue, _maxSampleValue);
        return Mathf.PerlinNoise(randomX, randomY);
    }

    private Vector3 GetRandomDisplacementFromStartPosition()
    {
        float xScaledDisplacement = _cameraShakeMagnitudeX * Random.Range(-1f, 1f) * GetRandomPerlinNoiseSamplePointFromSampleArea() + _startPosition.x;

        float yScaledDisplacement = _cameraShakeMagnitudeY * Random.Range(-1f, 1f) * GetRandomPerlinNoiseSamplePointFromSampleArea() + _startPosition.y;
        
        return new Vector3(
            xScaledDisplacement, 
            yScaledDisplacement, 
            _startPosition.z
            );
    }
    #endregion
}
