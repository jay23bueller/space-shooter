using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private TMP_Text _scoreText;
    [SerializeField]
    private Image _livesImage;
    [SerializeField]
    private Sprite[] _livesSprites;
    [SerializeField]
    private GameObject _gameOverGO;
    [SerializeField]
    private GameObject _restartGO;
    [SerializeField]
    private GameManager _gameManager;
    [SerializeField]
    private TMP_Text _ammoText;
    #endregion
    #region UnityMethods
    // Start is called before the first frame update

    #endregion
    #region Methods
    public void UpdateScoreText(int score)
    {
        _scoreText.text = $"<b>SCORE: {score}</b>";
    }

    public void UpdateAmmoText(int ammoCount)
    {
        _ammoText.text = $"<b>AMMO: {ammoCount}</b>";
    }

    public void UpdateLivesImage(int livesRemaining)
    {
        if(livesRemaining >= 0)
            _livesImage.sprite = _livesSprites[livesRemaining];
    }

    public void DisplayGameOver()
    {
        StartCoroutine(FlickerGameOverTextRoutine());
    }

    private IEnumerator FlickerGameOverTextRoutine()
    {
        for(int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(.3f);
            _gameOverGO.SetActive(!_gameOverGO.activeSelf);
        }

        StartCoroutine(EnableRestartRoutine());
    }



    private IEnumerator EnableRestartRoutine()
    {
        yield return new WaitForSeconds(1f);
        _restartGO.SetActive(true);
        _gameManager.GameOver();
    }

    #endregion
}
