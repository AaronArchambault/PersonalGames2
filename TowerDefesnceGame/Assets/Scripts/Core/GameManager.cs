
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
 
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
 
    [Header("Player Stats")]
    public int startingLives = 20;
    public int startingGold  = 150;
 
    public int  Lives    { get; private set; }
    public int  Gold     { get; private set; }
    public int  Wave     { get; private set; }
    public bool GameOver { get; private set; }
 
    [Header("Wave Tracking")]
    public int  GoldEarnedThisWave  { get; private set; }
    public int  LivesLostThisWave   { get; private set; }
    public bool PerfectRun          { get; private set; } // no lives lost ever
 
    [Header("Screen Flash")]
    public float flashDuration = 0.12f;
 
    // Events
    public event System.Action<int> OnLivesChanged;
    public event System.Action<int> OnGoldChanged;
    public event System.Action<int> OnWaveChanged;
    public event System.Action      OnGameOver;
    public event System.Action<int> OnGoldEarnedThisWaveChanged;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }
 
    void Start()
    {
        Lives              = startingLives;
        Gold               = startingGold;
        Wave               = 0;
        GameOver           = false;
        GoldEarnedThisWave = 0;
        LivesLostThisWave  = 0;
        PerfectRun         = true;
 
        // Subscribe to wave events to reset per-wave counters
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart    += _ => ResetWaveCounters();
            WaveManager.Instance.OnWaveComplete += OnWaveCompleted;
        }
    }
 
    // ── Gold ──────────────────────────────────────────────────
 
    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
 
    public void EarnGold(int amount)
    {
        Gold               += amount;
        GoldEarnedThisWave += amount;
        OnGoldChanged?.Invoke(Gold);
        OnGoldEarnedThisWaveChanged?.Invoke(GoldEarnedThisWave);
    }
 
    // ── Lives ─────────────────────────────────────────────────
 
    public void LoseLife(int amount = 1)
    {
        Lives             = Mathf.Max(0, Lives - amount);
        LivesLostThisWave += amount;
        PerfectRun        = false;
        OnLivesChanged?.Invoke(Lives);
 
        // Screen flash red
        if (AccessibilitySettings.ScreenFlash)
            StartCoroutine(ScreenFlash(new Color(1f, 0f, 0f, 0.35f), flashDuration));
 
        // Scale shake with damage
        CameraShake.Instance?.Shake(0.08f * amount, 0.3f);
 
        if (Lives <= 0) TriggerGameOver();
    }
 
    public void AddLives(int amount)
    {
        Lives += amount;
        OnLivesChanged?.Invoke(Lives);
        StartCoroutine(ScreenFlash(new Color(0f, 1f, 0f, 0.25f), 0.15f));
    }
 
    // ── Hit Stop ──────────────────────────────────────────────
 
    public void HitStop(float duration = 0.05f)
    {
        StartCoroutine(DoHitStop(duration));
    }
 
    IEnumerator DoHitStop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
 
    // ── Screen Flash ──────────────────────────────────────────
 
    public IEnumerator ScreenFlash(Color color, float duration)
    {
        if (BrightnessOverlay.Instance == null) yield break;
        // We use ColorFlash rather than brightness for colored flashes
        var overlay = BrightnessOverlay.Instance;
        overlay.SetColor(color);
        float t = 0;
        while (t < duration)
        {
            float alpha = Mathf.Lerp(color.a, 0f, t / duration);
            overlay.SetColorAlpha(alpha);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        overlay.SetColorAlpha(0f);
    }
 
    // ── Wave ──────────────────────────────────────────────────
 
    public void SetWave(int w)
    {
        Wave = w;
        OnWaveChanged?.Invoke(Wave);
    }
 
    void ResetWaveCounters()
    {
        GoldEarnedThisWave = 0;
        LivesLostThisWave  = 0;
    }
 
    void OnWaveCompleted()
    {
        // Star rating for this wave
        int stars = LivesLostThisWave == 0 ? 3
                  : LivesLostThisWave <= 2 ? 2
                  : 1;
        StarRatingSystem.Instance?.RegisterWave(Wave, stars);
        MetagameManager.Instance?.AddXP(stars * 50);
    }
 
    // ── Game Over ─────────────────────────────────────────────
 
    void TriggerGameOver()
    {
        GameOver = true;
        // Save star rating / metagame progress before freeze
        StarRatingSystem.Instance?.SaveRatings();
        MetagameManager.Instance?.SaveProgress();
        StartCoroutine(GameOverSequence());
    }
 
    IEnumerator GameOverSequence()
    {
        // Brief dramatic pause before game over screen
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
    }
 
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}