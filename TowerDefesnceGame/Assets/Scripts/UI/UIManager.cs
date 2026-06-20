using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI waveText;
    public Image speedIndicatorIcon;

    [Header("Wave")]
    public Button startWaveButton;
    public TextMeshProUGUI waveButtonText;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public TextMeshProUGUI gameOverWaveText;

    [Header("Announce")]
    public TextMeshProUGUI announceText;
    private Coroutine announceRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /*void OnEnable()
    {
        GameManager.Instance.OnLivesChanged += UpdateLives;
        GameManager.Instance.OnGoldChanged  += UpdateGold;
        GameManager.Instance.OnWaveChanged  += UpdateWave;
        GameManager.Instance.OnGameOver     += ShowGameOver;
        WaveManager.Instance.OnWaveStart    += OnWaveStart;
        WaveManager.Instance.OnWaveComplete += OnWaveComplete;
        WaveManager.Instance.OnAllWavesComplete += OnVictory;
    }

    void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnLivesChanged -= UpdateLives;
        GameManager.Instance.OnGoldChanged  -= UpdateGold;
        GameManager.Instance.OnWaveChanged  -= UpdateWave;
        GameManager.Instance.OnGameOver     -= ShowGameOver;
        WaveManager.Instance.OnWaveStart    -= OnWaveStart;
        WaveManager.Instance.OnWaveComplete -= OnWaveComplete;
        WaveManager.Instance.OnAllWavesComplete -= OnVictory;
    }

    void Start()
    {
        UpdateLives(GameManager.Instance.Lives);
        UpdateGold(GameManager.Instance.Gold);
        UpdateWave(GameManager.Instance.Wave);
        gameOverPanel.SetActive(false);
        if (victoryPanel) victoryPanel.SetActive(false);
    }*/

    // Scripts/UI/UIManager.cs — replace OnEnable and OnDisable

// DELETE OnEnable and OnDisable entirely, replace with:

void Start()
{
    // Subscribe to events here, after all Awake() calls have run
    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnLivesChanged += UpdateLives;
        GameManager.Instance.OnGoldChanged  += UpdateGold;
        GameManager.Instance.OnWaveChanged  += UpdateWave;
        GameManager.Instance.OnGameOver     += ShowGameOver;
    }
    else Debug.LogError("UIManager: GameManager.Instance is null!");

    if (WaveManager.Instance != null)
    {
        WaveManager.Instance.OnWaveStart        += OnWaveStart;
        WaveManager.Instance.OnWaveComplete     += OnWaveComplete;
        WaveManager.Instance.OnAllWavesComplete += OnVictory;
    }
    else Debug.LogError("UIManager: WaveManager.Instance is null!");

    // Initialize display
    if (GameManager.Instance != null)
    {
        UpdateLives(GameManager.Instance.Lives);
        UpdateGold(GameManager.Instance.Gold);
        UpdateWave(GameManager.Instance.Wave);
    }

    if (gameOverPanel) gameOverPanel.SetActive(false);
    if (victoryPanel)  victoryPanel.SetActive(false);
}

void OnDestroy()
{
    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnLivesChanged -= UpdateLives;
        GameManager.Instance.OnGoldChanged  -= UpdateGold;
        GameManager.Instance.OnWaveChanged  -= UpdateWave;
        GameManager.Instance.OnGameOver     -= ShowGameOver;
    }
    if (WaveManager.Instance != null)
    {
        WaveManager.Instance.OnWaveStart        -= OnWaveStart;
        WaveManager.Instance.OnWaveComplete     -= OnWaveComplete;
        WaveManager.Instance.OnAllWavesComplete -= OnVictory;
    }
}

void UpdateLives(int v) => livesText.text = $"Lives: {v}";
void UpdateGold(int v)  => goldText.text  = $"Gold: {v}";
    void UpdateWave(int v)  => waveText.text  = $"Wave {v}";

    public void UpdateSpeedIndicator(bool fast)
    {
        if (speedIndicatorIcon)
            speedIndicatorIcon.color = fast ? Color.yellow : Color.white;
    }

    void OnWaveStart(int wave)
    {
        startWaveButton.interactable = false;
        Announce($"Wave {wave}!", Color.yellow);
    }

   void OnWaveComplete()
{
    startWaveButton.interactable = true;

    // Pulse the button to draw attention
    var anim = startWaveButton.GetComponent<AnimatedButton>();
    anim?.TriggerPulseOnce();

    Announce("Wave clear!", Color.green);
}

    void OnVictory()
    {
        if (victoryPanel) victoryPanel.SetActive(true);
    }

    void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        gameOverWaveText.text = $"You survived {GameManager.Instance.Wave} wave(s)";
    }

    public void OnStartWaveButton() => WaveManager.Instance.StartNextWave();
    public void OnRestartButton()   => GameManager.Instance.RestartGame();

    // Big center announcement text
    public void Announce(string msg, Color color)
    {
        if (announceText == null) return;
        if (announceRoutine != null) StopCoroutine(announceRoutine);
        announceRoutine = StartCoroutine(ShowAnnounce(msg, color));
    }

    System.Collections.IEnumerator ShowAnnounce(string msg, Color color)
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

        t = 0;
        Color c = color;
        while (t < 0.4f)
        {
            c.a = Mathf.Lerp(1f, 0f, t / 0.4f);
            announceText.color = c;
            t += Time.deltaTime; yield return null;
        }
        announceText.gameObject.SetActive(false);
    }
}