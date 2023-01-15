using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _backgroundMusicAudioSource;
    [SerializeField]
    private AudioSource _gameOverMusicAudioSource;
    [SerializeField]
    private float _volumeFadeRate;

    public void GameOverAudio()
    {
        StartCoroutine(GameOverAudioRoutine());
    }

    private IEnumerator GameOverAudioRoutine()
    {
        yield return new WaitForSeconds(.5f);
        while(_backgroundMusicAudioSource.volume > 0)
        {
            yield return null;
            _backgroundMusicAudioSource.volume -= _volumeFadeRate * Time.deltaTime;
        }

        
        _gameOverMusicAudioSource.Play();
    }
}
