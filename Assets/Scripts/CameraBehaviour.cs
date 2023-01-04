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
    private Vector3 _startPosition;
    private bool _canShake = true;
    #endregion

    #region UnityMethods
    private void Start()
    {
        _startPosition = transform.position;
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
        for(int i = 0; i < 15; i++)
        {
            Vector3 displacement = GetRandomDisplacementFromStartPosition();
            transform.position = displacement;
            yield return new WaitForSeconds(.05f);
            transform.position = _startPosition;
        }
        _canShake = true;
    }

    private float GetRandomPerlinNoiseSamplePoint()
    {
        return  Mathf.PerlinNoise(Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    private Vector3 GetRandomDisplacementFromStartPosition()
    {
        return new Vector3(
            _cameraShakeMagnitudeX * Random.Range(-1f, 1f) * GetRandomPerlinNoiseSamplePoint() +_startPosition.x, 
            _cameraShakeMagnitudeY * Random.Range(-1f, 1f) * GetRandomPerlinNoiseSamplePoint() +_startPosition.y, 
            _startPosition.z
            );
    }
    #endregion
}
