using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private AudioSource _backgroundMusicAudioSource;

    [Header("Game Over Music")]
    [SerializeField]
    private AudioSource _gameOverMusicAudioSource;
    [SerializeField]
    private float _gameOverVolumeFadeRate = .03f;

    [Header("Boss Music")]
    [SerializeField]
    private float _bossVolumeFadeOutRate = .06f;
    [SerializeField]
    private float _bossVolumeFadeInRate = .03f;
    [SerializeField]
    private AudioClip _bossMusicClip;

    [Header("Win Music")]
    [SerializeField]
    private AudioClip _winMusicClip;
    [SerializeField]
    private float _winVolumeFadeOutRate = .06f;
    [SerializeField]
    private float _winVolumeFadeInRate = .03f;
    #endregion

    #region Methods
    public void GameOverAudio()
    {
        StopAllCoroutines();
        StartCoroutine(GameOverAudioRoutine());
    }

    public void StartWinMusic()
    {
        StartCoroutine(WinMusicRoutine());
    }

    private IEnumerator WinMusicRoutine()
    {
        while (_backgroundMusicAudioSource.volume > 0)
        {
            yield return null;
            _backgroundMusicAudioSource.volume = Mathf.Clamp(_backgroundMusicAudioSource.volume - (_winVolumeFadeOutRate * Time.deltaTime), 0f, 1.0f);
        }

        _backgroundMusicAudioSource.clip = _winMusicClip;
        _backgroundMusicAudioSource.loop = false;
        _backgroundMusicAudioSource.Play();
        yield return new WaitForSeconds(.5f);
        while (_backgroundMusicAudioSource.volume < 1.0f)
        {
            yield return null;
            _backgroundMusicAudioSource.volume = Mathf.Clamp(_backgroundMusicAudioSource.volume + (_winVolumeFadeInRate * Time.deltaTime), 0f, 1.0f); ;
        }

    }

    public void StartBossMusic()
    {
        StartCoroutine(BossAudioRoutine());
    }

    private IEnumerator BossAudioRoutine()
    {
        while (_backgroundMusicAudioSource.volume > 0)
        {
            yield return null;
            _backgroundMusicAudioSource.volume = Mathf.Clamp(_backgroundMusicAudioSource.volume- (_bossVolumeFadeOutRate * Time.deltaTime),0f,1.0f);
        }

        _backgroundMusicAudioSource.clip = _bossMusicClip;
        _backgroundMusicAudioSource.Play();
        yield return new WaitForSeconds(.5f);
        while (_backgroundMusicAudioSource.volume < 1.0f)
        {
            yield return null;
            _backgroundMusicAudioSource.volume = Mathf.Clamp(_backgroundMusicAudioSource.volume + (_bossVolumeFadeInRate * Time.deltaTime), 0f, 1.0f); ;
        }

    }

    private IEnumerator GameOverAudioRoutine()
    {
        

        while(_backgroundMusicAudioSource.volume > 0)
        {
            yield return null;
            _backgroundMusicAudioSource.volume = Mathf.Clamp(_backgroundMusicAudioSource.volume - (_gameOverVolumeFadeRate * Time.deltaTime), 0f, 1.0f);
        }

        
        _gameOverMusicAudioSource.Play();
    }
    #endregion
}
