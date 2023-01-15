using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public enum WeaponIconName
    {
        Laser = 0,
        TripleShot = 1,
        HomingMissile = 2
    }
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
    private Image _disrupted;
    private Coroutine _disruptedTextRoutine;
    [SerializeField]
    private Sprite[] _weaponIcons;
    [SerializeField]
    private Image _ammoImage;
    [SerializeField]
    private Image _ammoFillImage;
    private float _weaponCooldownTimer;
    private float _weaponCooldownDuration = 5f;
    #endregion
    #region UnityMethods
    // Start is called before the first frame update

    private void Update()
    {
        if(Time.time < _weaponCooldownTimer)
        {
            Vector2 previousLocalScale = _ammoFillImage.rectTransform.localScale;
            _ammoFillImage.rectTransform.localScale = new Vector2(previousLocalScale.x, Mathf.Clamp(previousLocalScale.y - (1 / _weaponCooldownDuration * Time.deltaTime),0f,1f));

        }
    }
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
            _disruptedTextRoutine = StartCoroutine(FlickerTextRoutine(_disrupted.gameObject, false));
        } else
        {
            if (_disruptedTextRoutine != null)
                StopCoroutine(_disruptedTextRoutine);
            _disrupted.gameObject.SetActive(false);
        }
    }

    public void UpdateAmmoImage(WeaponIconName name)
    {
        _ammoImage.sprite = _weaponIcons[(int)name];
        switch(name)
        {
            case WeaponIconName.TripleShot:
            case WeaponIconName.HomingMissile:
                _ammoFillImage.rectTransform.localScale = new Vector2(1f, 1f);
                _weaponCooldownTimer = Time.time + _weaponCooldownDuration;
            break;
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
        _disrupted.gameObject.SetActive(false);
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
