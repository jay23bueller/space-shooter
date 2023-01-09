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
    [SerializeField]
    private TMP_Text _disruptedText;
    private Coroutine _disruptedTextRoutine;
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

    public void UpdateDisruptionText(bool show)
    {
        if (show)
        {
            _disruptedTextRoutine = StartCoroutine(FlickerTextRoutine(_disruptedText.gameObject, false));
        } else
        {
            if (_disruptedTextRoutine != null)
                StopCoroutine(_disruptedTextRoutine);
            _disruptedText.gameObject.SetActive(false);
        }
    }

    public void DisplayWinText()
    {
        _waveText.text = "YOU WIN";
        StartCoroutine(FlickerTextRoutine(_waveText.gameObject, true));
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
        StopAllCoroutines();
        _disruptedText.gameObject.SetActive(false);
        _ammoText.gameObject.SetActive(false);
        _thrusterSlider.gameObject.SetActive(false);
        StartCoroutine(FlickerTextRoutine(_gameOverGO, true));
    }

    private IEnumerator FlickerTextRoutine(GameObject objectToFlicker, bool restart)
    {
        for(int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(.3f);
            objectToFlicker.SetActive(!objectToFlicker.activeSelf);
        }

        if(restart)
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
