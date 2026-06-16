using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach this to a UI panel that contains the prestige button and confirmation dialog.
// Wire up all references in the Inspector.
public class PrestigeUIController : MonoBehaviour
{
    [Header("Prestige Button (always visible in HUD)")]
    public Button    prestigeButton;
    public TMP_Text  prestigeButtonLabel;   // shows "PRESTIGE" or locked reason

    [Header("Confirmation Panel")]
    public GameObject confirmPanel;
    public TMP_Text   confirmRewardText;    // "You will earn +3 Cowrium"
    public TMP_Text   confirmMultiplierText;// "New income bonus: +130%"
    public TMP_Text   confirmWarningText;   // "All cows and coins will be lost!"
    public Button     confirmYesButton;
    public Button     confirmNoButton;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TMP_Text   resultCowriumText;    // "+3 Cowrium earned!"
    public TMP_Text   resultPrestigeCount;  // "Prestige #2"
    public TMP_Text   resultTitleText;      // "Moo Master"
    public TMP_Text   resultMultiplierText; // "Income bonus: +130%"
    public Button     resultCloseButton;

    [Header("HUD displays (update on prestige)")]
    public TMP_Text   cowriumLabel;         // always-visible Cowrium count
    public TMP_Text   prestigeTitleLabel;   // player's current title
    public TMP_Text   incomeMultiplierLabel;// e.g. "+30% income"

    // ── Unity lifecycle ────────────────────────────────────────────────────────
    void OnEnable()
    {
        PrestigeManager.Instance.OnCowriumChanged += RefreshHUD;
        PrestigeManager.Instance.OnPrestiged      += RefreshHUD;
        GameManager.Instance.OnCPSChanged         += _ => RefreshPrestigeButton();

        confirmYesButton.onClick.AddListener(OnConfirmYes);
        confirmNoButton .onClick.AddListener(OnConfirmNo);
        resultCloseButton.onClick.AddListener(OnResultClose);
        prestigeButton  .onClick.AddListener(OnPrestigeButtonPressed);
    }

    void OnDisable()
    {
        if (PrestigeManager.Instance)
        {
            PrestigeManager.Instance.OnCowriumChanged -= RefreshHUD;
            PrestigeManager.Instance.OnPrestiged      -= RefreshHUD;
        }
        confirmYesButton .onClick.RemoveAllListeners();
        confirmNoButton  .onClick.RemoveAllListeners();
        resultCloseButton.onClick.RemoveAllListeners();
        prestigeButton   .onClick.RemoveAllListeners();
    }

    void Start()
    {
        confirmPanel.SetActive(false);
        resultPanel .SetActive(false);
        RefreshHUD();
        RefreshPrestigeButton();
    }

    // ── Button handlers ────────────────────────────────────────────────────────
    void OnPrestigeButtonPressed()
    {
        if (!PrestigeManager.Instance.CanPrestige())
        {
            UIManager.Instance?.ShowMessage(
                $"Need a tier {PrestigeManager.Instance.minimumTierToPrestige}+ cow to prestige!");
            return;
        }
        OpenConfirmPanel();
    }

    void OnConfirmYes()
    {
        confirmPanel.SetActive(false);
        PrestigeManager.Instance.ExecutePrestige();
    }

    void OnConfirmNo() => confirmPanel.SetActive(false);

    void OnResultClose() => resultPanel.SetActive(false);

    // ── Panels ─────────────────────────────────────────────────────────────────
    void OpenConfirmPanel()
    {
        var pm     = PrestigeManager.Instance;
        int reward = pm.PreviewCowriumReward();
        float newMultiplier = 1f + ((pm.Cowrium + reward) * pm.incomeBoostPerCowrium);

        confirmRewardText    .text = $"You will earn +{reward} Cowrium";
        confirmMultiplierText.text = $"New income bonus: +{(newMultiplier - 1f) * 100f:F0}%";
        confirmWarningText   .text = "All cows and coins will be reset!";
        confirmPanel.SetActive(true);
    }

    // Called by PrestigeManager event via UIManager
    public void ShowResult(int reward, int prestigeCount, float multiplier)
    {
        resultCowriumText  .text = $"+{reward} Cowrium earned!";
        resultPrestigeCount.text = $"Prestige #{prestigeCount}";
        resultTitleText    .text = PrestigeManager.Instance.GetTitle();
        resultMultiplierText.text = $"Income bonus: +{(multiplier - 1f) * 100f:F0}%";
        resultPanel.SetActive(true);
    }

    // ── HUD refresh ────────────────────────────────────────────────────────────
    void RefreshHUD()
    {
        var pm = PrestigeManager.Instance;
        if (cowriumLabel)          cowriumLabel.text          = $"{pm.Cowrium} Cowrium";
        if (prestigeTitleLabel)    prestigeTitleLabel.text    = pm.GetTitle();
        if (incomeMultiplierLabel) incomeMultiplierLabel.text =
            $"+{(pm.IncomeMultiplier - 1f) * 100f:F0}% income";
        RefreshPrestigeButton();
    }

    void RefreshPrestigeButton()
    {
        bool canPrestige = PrestigeManager.Instance.CanPrestige();
        prestigeButton.interactable = canPrestige;
        if (prestigeButtonLabel)
            prestigeButtonLabel.text = canPrestige
                ? $"PRESTIGE (+{PrestigeManager.Instance.PreviewCowriumReward()} Cowrium)"
                : $"Need tier {PrestigeManager.Instance.minimumTierToPrestige}+ cow";
    }
}
