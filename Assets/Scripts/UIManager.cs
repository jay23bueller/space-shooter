using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField]
    private Slider _thrusterSlider;
    [SerializeField]
    private GameObject _thrusterFillGO;
    private int _maxAmmoCount = 15;
    [SerializeField]
    private TMP_Text _waveText;
    #endregion
    #region UnityMethods
    // Start is called before the first frame update

    #endregion
    #region Methods
    public void UpdateScoreText(int score)
    {
        _scoreText.text = $"<b>SCORE: {score}</b>";
    }

    public void UpdateWaveText(int wave)
    {
        _waveText.text = $"WAVE: {wave}";
    }

    public void DisplayWinText()
    {
        _waveText.text = "YOU WIN";
        StartCoroutine(FlickerTextRoutine(_waveText.gameObject));
        StartCoroutine(EnableRestartRoutine());
    }

    public void DisplayWaveText(bool display)
    {
        _waveText.gameObject.SetActive(display);
    }

    public void SetAmmoMaxCount(int maxAmmoCount)
    {
        _maxAmmoCount = maxAmmoCount;
    }

    public void UpdateAmmoText(int ammoCount)
    {
        _ammoText.text = $"<b>AMMO: {ammoCount}/{_maxAmmoCount}</b>";
    }

    public void UpdateLivesImage(int livesRemaining)
    {
        if(livesRemaining >= 0)
            _livesImage.sprite = _livesSprites[livesRemaining];
    }

    public void UpdateThrusterSlider(float value, bool isResetting)
    {
        if (isResetting)
            _thrusterFillGO.GetComponent<Image>().color = Color.red;
        else
            _thrusterFillGO.GetComponent<Image>().color = Color.HSVToRGB(.43f,1f,1f);
        _thrusterSlider.value = value;
    }

    public void DisplayGameOver()
    {
        StartCoroutine(FlickerTextRoutine(_gameOverGO));
    }

    private IEnumerator FlickerTextRoutine(GameObject objectToFlicker)
    {
        for(int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(.3f);
            objectToFlicker.SetActive(!objectToFlicker.activeSelf);
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
