using System.Collections;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public TMP_Text coinLabel;
    public TMP_Text cpsLabel;

    [Header("Popups")]
    public GameObject evolutionPopupPanel;
    public TMP_Text   evolutionPopupName;
    public TMP_Text   evolutionPopupTier;

    [Header("Messages")]
    public TMP_Text   messageLabel;

    [Header("Offline Earnings")]
    public GameObject offlinePanel;
    public TMP_Text   offlineAmountLabel;
    public TMP_Text   offlineTimeLabel;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        GameManager.Instance.OnCoinsChanged += UpdateCoins;
        GameManager.Instance.OnCPSChanged   += UpdateCPS;
    }

    void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnCoinsChanged -= UpdateCoins;
        GameManager.Instance.OnCPSChanged   -= UpdateCPS;
    }

    void UpdateCoins(double c) =>
        coinLabel.text = ShopController.FormatCoins(c);

    void UpdateCPS(double cps) =>
        cpsLabel.text  = $"{ShopController.FormatCoins(cps)}/s";

    // ── Evolution popup ───────────────────────────────────────────────────────
    public void ShowEvolutionPopup(CowData newCow)
    {
        if (evolutionPopupPanel == null) return;
        evolutionPopupName.text = newCow.cowName;
        evolutionPopupTier.text = $"Tier {newCow.tier}";
        evolutionPopupPanel.SetActive(true);
        StartCoroutine(HideAfter(evolutionPopupPanel, 2.5f));
    }

    // ── Offline earnings ──────────────────────────────────────────────────────
    public void ShowOfflineEarnings(double amount, double seconds)
    {
        if (offlinePanel == null) return;
        offlineAmountLabel.text = $"+{ShopController.FormatCoins(amount)} coins";
        int hours   = Mathf.FloorToInt((float)seconds / 3600);
        int minutes = Mathf.FloorToInt((float)(seconds % 3600) / 60);
        offlineTimeLabel.text = $"While away for {hours}h {minutes}m";
        offlinePanel.SetActive(true);
    }

    public void CloseOfflinePanel() => offlinePanel?.SetActive(false);

    // ── Toast messages ────────────────────────────────────────────────────────
    Coroutine _msgCoroutine;

    public void ShowMessage(string text)
    {
        if (messageLabel == null) return;
        if (_msgCoroutine != null) StopCoroutine(_msgCoroutine);
        messageLabel.text = text;
        messageLabel.gameObject.SetActive(true);
        _msgCoroutine = StartCoroutine(HideMessageAfter(2f));
    }

    IEnumerator HideMessageAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageLabel.gameObject.SetActive(false);
    }

    IEnumerator HideAfter(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(false);
    }
}
