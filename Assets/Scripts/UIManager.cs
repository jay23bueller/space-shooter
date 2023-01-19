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
    [SerializeField]
    private TMP_Text _streakText;
    [SerializeField]
    private TMP_Text _thrustText;
    private float _weaponCooldownTimer;
    private float _weaponCooldownDuration = 5f;
    private Animator _streakTextAnimator;
    private Animator _scoreTextAnimator;
    #endregion
    #region UnityMethods
    private void Start()
    {
        _streakTextAnimator = _streakText.GetComponent<Animator>();
        _scoreTextAnimator = _scoreText.GetComponent<Animator>();
    }

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

    public void UpdateStreakText(int streak, bool shake)
    {
        _streakText.text = $"<b>STREAK: {streak}</b>";

        if(shake)
        {
            _streakTextAnimator.SetTrigger("triggerAnimation");
        }
    }

    public void UpdateThrusterText(bool isAccelerated)
    {
        _thrustText.text = isAccelerated ? "BOOSTED ENERGY" : "ENERGY";
        if (isAccelerated)
            StartCoroutine(FlickerTextRoutine(_thrustText.gameObject, false, false));

    }
    public void UpdateScoreText(int score, bool shake)
    {
        _scoreText.text = $"<b>SCORE: {score}</b>";

        if (shake)
        {
            _scoreTextAnimator.SetTrigger("triggerAnimation");
        }
    }


    public void UpdateWaveText(int wave)
    {
        _waveText.text = $"WAVE: {wave}";
    }

    public void UpdateDisruptionText(bool show)
    {
        if (show)
        {
            _disruptedTextRoutine = StartCoroutine(FlickerTextRoutine(_disrupted.gameObject, false, true));
        } else
        {
            if (_disruptedTextRoutine != null)
                StopCoroutine(_disruptedTextRoutine);
            _disrupted.gameObject.SetActive(false);
        }
    }

    public void UpdateWeaponCooldownDuration(float newWeaponCooldownDuration)
    {
        _weaponCooldownDuration = newWeaponCooldownDuration;
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
        StartCoroutine(FlickerTextRoutine(_waveText.gameObject, true, true));
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
        StartCoroutine(FlickerTextRoutine(_gameOverGO, true, true));
    }

    private IEnumerator FlickerTextRoutine(GameObject objectToFlicker, bool restart, bool odd)
    {
        int flickerAmount = odd ? 5 : 6;
        for(int i = 0; i < flickerAmount; i++)
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
