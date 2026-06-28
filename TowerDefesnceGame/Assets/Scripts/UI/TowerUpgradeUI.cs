
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class TowerUpgradeUI : MonoBehaviour
{
    public static TowerUpgradeUI Instance { get; private set; }
 
    [Header("Panel")]
    public GameObject      panel;
    public TextMeshProUGUI towerNameText;
    public Button          closeButton;    // wire to a X button in the panel
 
    [Header("Path A")]
    public TextMeshProUGUI pathANameText;
    public TextMeshProUGUI pathADescText;  // optional description
    public TextMeshProUGUI pathACostText;
    public Button          pathAButton;
 
    [Header("Path B")]
    public TextMeshProUGUI pathBNameText;
    public TextMeshProUGUI pathBDescText;
    public TextMeshProUGUI pathBCostText;
    public Button          pathBButton;
 
    [Header("Sell")]
    public TextMeshProUGUI sellValueText;
    public Button          sellButton;
 
    [Header("Stats Display (optional)")]
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI towerStatsText;
 
    private Tower selectedTower;
    private bool  isOpen = false;

    [Header("Targeting")]
public TextMeshProUGUI targetModeText;
public Button          targetModeButton;

public void OnClickCycleTarget()
{
    if (selectedTower == null) return;
    selectedTower.CycleTargetMode();
    if (targetModeText)
        targetModeText.text = $"Target: {selectedTower.targetMode}";
}

 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
 
        if (panel) panel.SetActive(false);
 
        // Wire close button if assigned
        if (closeButton) closeButton.onClick.AddListener(Hide);
    }
 
    void OnEnable()
    {
        // Wire Escape key to close the panel
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnCancelPerformed += OnEscapePressed;
    }
 
    void OnDisable()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnCancelPerformed -= OnEscapePressed;
    }
 
    void OnEscapePressed()
    {
        if (isOpen) Hide();
    }
 
    // ── Open / Close ──────────────────────────────────────────
 
    public void Show(Tower tower)
    {
        // Clicking the same tower again while open just refreshes
        if (isOpen && selectedTower == tower)
        {
            Refresh();
            return;
        }
 
        // Deselect previous tower
        if (selectedTower != null && selectedTower != tower)
            selectedTower.SetSelected(false);
 
        selectedTower = tower;
        isOpen        = true;
        panel.SetActive(true);
        Refresh();
    }
 
    public void Hide()
    {
        if (selectedTower != null)
            selectedTower.SetSelected(false);
 
        selectedTower = null;
        isOpen        = false;
 
        if (panel) panel.SetActive(false);
    }
 
    // ── Refresh ───────────────────────────────────────────────
 
    void Refresh()
    {
        if (selectedTower == null) { Hide(); return; }
 
        // Tower name
        if (towerNameText)
        {
            string name = selectedTower.upgradeData != null
                ? selectedTower.upgradeData.towerName
                : selectedTower.name.Replace("(Clone)", "").Trim();
            towerNameText.text = name;
        }
 
        // Kill count
        if (killCountText)
            killCountText.text = $"Kills: {selectedTower.killCount}";
 
        // Paths
        RefreshPath(0, selectedTower.pathALevel,
            pathANameText, pathADescText, pathACostText, pathAButton);
        RefreshPath(1, selectedTower.pathBLevel,
            pathBNameText, pathBDescText, pathBCostText, pathBButton);
 
        // Sell
        if (sellValueText)
        {
            int sellVal = selectedTower.GetSellValue();
            sellValueText.text = $"Sell  +{sellVal}g";
        }
        if (sellButton) sellButton.interactable = true;


        if (targetModeText && selectedTower != null)
    targetModeText.text = $"Target: {selectedTower.targetMode}";

    }
 
    void RefreshPath(int path, int level,
        TextMeshProUGUI nameText,
        TextMeshProUGUI descText,
        TextMeshProUGUI costText,
        Button btn)
    {
        if (selectedTower == null || selectedTower.upgradeData == null)
        {
            if (nameText) nameText.text    = "—";
            if (descText) descText.text    = "";
            if (costText) costText.text    = "";
            if (btn)      btn.interactable = false;
            return;
        }
 
        bool maxed = level >= 3;
 
        if (maxed)
        {
            if (nameText) nameText.text    = "MAX";
            if (descText) descText.text    = "";
            if (costText) costText.text    = "";
            if (btn)      btn.interactable = false;
            return;
        }
 
        var tier = path == 0
            ? selectedTower.upgradeData.pathA[level]
            : selectedTower.upgradeData.pathB[level];
 
        if (nameText) nameText.text = tier.upgradeName;
        if (descText) descText.text = tier.description;
 
        int upgradeCost = selectedTower.GetUpgradeCost(path);
        if (costText) costText.text = $"{upgradeCost}g";
 
        // Grey out if can't afford
        bool canAfford = GameManager.Instance != null &&
                         GameManager.Instance.Gold >= upgradeCost;
        if (btn) btn.interactable = canAfford;
    }
 
    // ── Button Callbacks (wire these in Inspector OnClick) ────
 
    public void OnClickUpgradeA()
    {
        if (selectedTower == null) return;
 
        bool success = selectedTower.TryUpgrade(0);
        if (success)
        {
            Refresh();
        }
        else
        {
            // Can't afford — shake the button
            pathAButton?.GetComponent<AnimatedButton>()?.PlayDenyShake();
            FloatingTextPool.Instance?.Spawn(
                panel.transform.position, "Not enough gold!", Color.red);
        }
    }
 
    public void OnClickUpgradeB()
    {
        if (selectedTower == null) return;
 
        bool success = selectedTower.TryUpgrade(1);
        if (success)
        {
            Refresh();
        }
        else
        {
            pathBButton?.GetComponent<AnimatedButton>()?.PlayDenyShake();
            FloatingTextPool.Instance?.Spawn(
                panel.transform.position, "Not enough gold!", Color.red);
        }
    }
 
    public void OnClickSell()
    {
        if (selectedTower == null) return;
 
        int refund = selectedTower.GetSellValue();
        GameManager.Instance.EarnGold(refund);
 
        FloatingTextPool.Instance?.Spawn(
            selectedTower.transform.position + Vector3.up,
            $"+{refund}g", Color.yellow);
 
        Destroy(selectedTower.gameObject);
 
        // Clear AFTER destroying
        selectedTower = null;
        isOpen        = false;
        if (panel) panel.SetActive(false);
    }
 
    // Called by OnClick in TowerPlacer when escape/right-click
    // is pressed while NOT placing — closes the panel
    public void OnClickClose() => Hide();
}