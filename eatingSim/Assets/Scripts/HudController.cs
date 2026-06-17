using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// HUDController — drives the in-game HUD.
/// Attach to the Canvas GameObject in the Game scene.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("HUD Elements")]
    public Image      massBarFill;       // Image → Fill Method: Horizontal
    public TMP_Text   massLabel;
    public TMP_Text   sizeLabel;
    public TMP_Text   tierLabel;         // centre-screen tier announce
    public Image      damageFlash;       // full-screen red Image, alpha 0 normally

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Settings")]
    public float barMaxMass    = 2000f;
    public float flashDuration = 0.3f;

    private PlayerCreature player;
    private Coroutine      tierRoutine;
    private Coroutine      flashRoutine;

    void Start()
    {
        player = Object.FindAnyObjectByType<PlayerCreature>();

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.onMassChanged.AddListener(OnMassChanged);
            gm.onPlayerDamaged.AddListener(OnPlayerDamaged);
            gm.onTierUnlocked.AddListener(ShowTierUnlock);
            gm.onGameOver.AddListener(ShowGameOver);
            gm.onVictory.AddListener(ShowVictory);
        }

        if (tierLabel)    tierLabel.gameObject.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (victoryPanel)  victoryPanel.SetActive(false);
        if (damageFlash)  { var c = damageFlash.color; c.a = 0f; damageFlash.color = c; }
    }

    void Update()
    {
        if (player && sizeLabel)
            sizeLabel.text = $"Size: {player.CurrentSize:F1}x";
    }

    // ── Callbacks ──────────────────────────────────────────
    void OnMassChanged(float massEaten, float totalMass)
    {
        if (massLabel) massLabel.text = $"Mass: {Mathf.RoundToInt(totalMass)}";
        if (massBarFill) massBarFill.fillAmount = Mathf.Clamp01(totalMass / barMaxMass);
    }

    void OnPlayerDamaged(float massDamage, float totalMass)
    {
        if (massLabel) massLabel.text = $"Mass: {Mathf.RoundToInt(totalMass)}";
        if (massBarFill) massBarFill.fillAmount = Mathf.Clamp01(totalMass / barMaxMass);

        // Red screen flash
        if (damageFlash != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(DamageFlashRoutine());
        }
    }

    IEnumerator DamageFlashRoutine()
    {
        Color c    = damageFlash.color;
        c.a        = 0.45f;
        damageFlash.color = c;
        float t    = 0f;
        while (t < flashDuration)
        {
            t   += Time.deltaTime;
            c.a  = Mathf.Lerp(0.45f, 0f, t / flashDuration);
            damageFlash.color = c;
            yield return null;
        }
        c.a = 0f;
        damageFlash.color = c;
    }

    void ShowTierUnlock(string tierName)
    {
        if (tierLabel == null) return;
        if (tierRoutine != null) StopCoroutine(tierRoutine);
        tierRoutine = StartCoroutine(TierAnnounceRoutine(tierName));
    }

    IEnumerator TierAnnounceRoutine(string tierName)
    {
        tierLabel.text = $"Now eating: {tierName}!";
        tierLabel.gameObject.SetActive(true);

        CanvasGroup cg = tierLabel.GetComponent<CanvasGroup>();
        if (cg == null) cg = tierLabel.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float t  = 0f;
        while (t < 0.4f) { t += Time.deltaTime; cg.alpha = t / 0.4f; yield return null; }
        cg.alpha = 1f;
        yield return new WaitForSeconds(2.5f);
        t = 0f;
        while (t < 0.4f) { t += Time.deltaTime; cg.alpha = 1f - t / 0.4f; yield return null; }
        tierLabel.gameObject.SetActive(false);
    }

    void ShowGameOver() { if (gameOverPanel) gameOverPanel.SetActive(true); }
    void ShowVictory()  { if (victoryPanel)  victoryPanel.SetActive(true);  }

    // ── Button callbacks ───────────────────────────────────
    public void OnRestartClicked()  => GameManager.Instance?.RestartGame();
    public void OnMainMenuClicked() => GameManager.Instance?.LoadMainMenu();
}