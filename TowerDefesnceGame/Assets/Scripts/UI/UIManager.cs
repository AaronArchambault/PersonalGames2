
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
 
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
 
    [Header("HUD")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI goldEarnedText;   // "Wave income: +Xg"
    public Image           speedIndicatorIcon;
 
    [Header("Wave")]
    public Button          startWaveButton;
    public TextMeshProUGUI waveButtonText;
 
    [Header("Panels")]
    public GameObject      gameOverPanel;
    public GameObject      victoryPanel;
    public TextMeshProUGUI gameOverWaveText;
 
    [Header("Announce")]
    public TextMeshProUGUI announceText;
    private Coroutine      announceRoutine;
 
    [Header("Theme UI")]
    public TextMeshProUGUI themeDescriptionText;
    public GameObject      themeDescriptionPanel;
    public Image           windIndicatorArrow;
    public TextMeshProUGUI windStrengthText;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
 
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged               += UpdateLives;
            GameManager.Instance.OnGoldChanged                += UpdateGold;
            GameManager.Instance.OnWaveChanged                += UpdateWave;
            GameManager.Instance.OnGameOver                   += ShowGameOver;
            GameManager.Instance.OnGoldEarnedThisWaveChanged  += UpdateGoldEarned;
 
            UpdateLives(GameManager.Instance.Lives);
            UpdateGold(GameManager.Instance.Gold);
            UpdateWave(GameManager.Instance.Wave);
        }
 
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart        += OnWaveStart;
            WaveManager.Instance.OnWaveComplete     += OnWaveComplete;
            WaveManager.Instance.OnAllWavesComplete += OnVictory;
        }
 
        if (gameOverPanel)         gameOverPanel.SetActive(false);
        if (victoryPanel)          victoryPanel.SetActive(false);
        if (themeDescriptionPanel) themeDescriptionPanel.SetActive(false);
        if (goldEarnedText)        goldEarnedText.text = "";
    }
 
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged              -= UpdateLives;
            GameManager.Instance.OnGoldChanged               -= UpdateGold;
            GameManager.Instance.OnWaveChanged               -= UpdateWave;
            GameManager.Instance.OnGameOver                  -= ShowGameOver;
            GameManager.Instance.OnGoldEarnedThisWaveChanged -= UpdateGoldEarned;
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart        -= OnWaveStart;
            WaveManager.Instance.OnWaveComplete     -= OnWaveComplete;
            WaveManager.Instance.OnAllWavesComplete -= OnVictory;
        }
    }
 
    void UpdateLives(int v)   { if (livesText)    livesText.text    = $"Lives: {v}"; }
    void UpdateGold(int v)    { if (goldText)     goldText.text     = $"Gold: {v}"; }
    void UpdateWave(int v)    { if (waveText)     waveText.text     = $"Wave {v}"; }
    void UpdateGoldEarned(int v) { if (goldEarnedText) goldEarnedText.text = $"+{v}g this wave"; }
 
    public void UpdateSpeedIndicator(bool fast)
    {
        if (speedIndicatorIcon) speedIndicatorIcon.color = fast ? Color.yellow : Color.white;
    }
 
    void OnWaveStart(int wave)
    {
        if (startWaveButton) startWaveButton.interactable = false;
        Announce($"Wave {wave}!", Color.yellow);
    }
 
    void OnWaveComplete()
    {
        if (startWaveButton) startWaveButton.interactable = true;
        startWaveButton?.GetComponent<AnimatedButton>()?.TriggerPulseOnce();
        Announce("Wave clear!", Color.green);
    }
 
    void OnVictory()  { if (victoryPanel)  victoryPanel.SetActive(true); }
 
    void ShowGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (gameOverWaveText && GameManager.Instance != null)
            gameOverWaveText.text = $"You survived {GameManager.Instance.Wave} wave(s)";
    }
 
    public void OnStartWaveButton() { WaveManager.Instance?.StartNextWave(); }
    public void OnRestartButton()   { GameManager.Instance?.RestartGame(); }
 
    public void Announce(string msg, Color color)
    {
        if (announceText == null) return;
        if (announceRoutine != null) StopCoroutine(announceRoutine);
        announceRoutine = StartCoroutine(ShowAnnounce(msg, color));
    }
 
    IEnumerator ShowAnnounce(string msg, Color color)
    {
        announceText.text  = msg;
        announceText.color = color;
        announceText.gameObject.SetActive(true);
 
        float t = 0;
        while (t < 0.15f)
        {
            announceText.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.1f, t / 0.15f);
            t += Time.deltaTime; yield return null;
        }
        announceText.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(1.5f);
 
        t = 0; Color c = color;
        while (t < 0.4f)
        {
            c.a = Mathf.Lerp(1f, 0f, t / 0.4f);
            announceText.color = c;
            t += Time.deltaTime; yield return null;
        }
        announceText.gameObject.SetActive(false);
    }
 
    public void ShowThemeDescription(string desc)
    {
        if (themeDescriptionPanel) themeDescriptionPanel.SetActive(true);
        if (themeDescriptionText)  themeDescriptionText.text = desc;
        StartCoroutine(HideThemeDescAfterDelay(5f));
    }
 
    IEnumerator HideThemeDescAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (themeDescriptionPanel) themeDescriptionPanel.SetActive(false);
    }
 
    public void ShowWindIndicator(Vector2 wind)
    {
        if (windIndicatorArrow == null) return;
        float angle = Mathf.Atan2(wind.y, wind.x) * Mathf.Rad2Deg;
        windIndicatorArrow.transform.rotation = Quaternion.Euler(0, 0, angle);
        if (windStrengthText) windStrengthText.text = $"Wind: {wind.magnitude:F1}";
    }
}