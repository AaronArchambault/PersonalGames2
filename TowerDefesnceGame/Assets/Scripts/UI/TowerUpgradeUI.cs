// In TowerUpgradeUI.cs — replace the full script

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerUpgradeUI : MonoBehaviour
{
    public static TowerUpgradeUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject      panel;
    public TextMeshProUGUI towerNameText;

    [Header("Path A")]
    public TextMeshProUGUI pathANameText;
    public TextMeshProUGUI pathACostText;
    public Button          pathAButton;

    [Header("Path B")]
    public TextMeshProUGUI pathBNameText;
    public TextMeshProUGUI pathBCostText;
    public Button          pathBButton;

    [Header("Sell")]
    public TextMeshProUGUI sellValueText;
    public Button          sellButton;

    private Tower selectedTower;
    private bool  isOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        panel.SetActive(false);
    }

    // ── Open / Close ──────────────────────────────────

    public void Show(Tower tower)
    {
        // If clicking the same tower that's already open, do nothing
        if (isOpen && selectedTower == tower) return;

        // Deselect previous
        if (selectedTower != null && selectedTower != tower)
            selectedTower.SetSelected(false);

        selectedTower = tower;
        isOpen = true;
        panel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        if (selectedTower != null)
            selectedTower.SetSelected(false);
        selectedTower = null;
        isOpen = false;
        panel.SetActive(false);
    }

    // ── Refresh display ───────────────────────────────

    void Refresh()
    {
        if (selectedTower == null) { Hide(); return; }

        // Tower name
        string name = selectedTower.upgradeData != null
            ? selectedTower.upgradeData.towerName
            : selectedTower.name.Replace("(Clone)", "").Trim();
        towerNameText.text = name;

        // Path A
        RefreshPath(
            path:       0,
            level:      selectedTower.pathALevel,
            nameText:   pathANameText,
            costText:   pathACostText,
            btn:        pathAButton);

        // Path B
        RefreshPath(
            path:       1,
            level:      selectedTower.pathBLevel,
            nameText:   pathBNameText,
            costText:   pathBCostText,
            btn:        pathBButton);

        // Sell value
        int sellVal = selectedTower.GetSellValue();
        sellValueText.text = $"Sell  +{sellVal}g" +
            (selectedTower.killCount > 0
                ? $"  ({selectedTower.killCount} kills)"
                : "");

        // Disable sell button if nothing to sell
        if (sellButton) sellButton.interactable = true;
    }

    void RefreshPath(int path, int level,
        TextMeshProUGUI nameText,
        TextMeshProUGUI costText,
        Button btn)
    {
        if (selectedTower == null || selectedTower.upgradeData == null)
        {
            if (nameText) nameText.text = "No data";
            if (costText) costText.text = "";
            if (btn)      btn.interactable = false;
            return;
        }

        bool maxed = level >= 3;

        if (maxed)
        {
            if (nameText) nameText.text = "MAX";
            if (costText) costText.text = "";
            if (btn)      btn.interactable = false;
            return;
        }

        var tier = path == 0
            ? selectedTower.upgradeData.pathA[level]
            : selectedTower.upgradeData.pathB[level];

        if (nameText) nameText.text = tier.upgradeName;

        int cost = selectedTower.GetUpgradeCost(path);
        if (costText) costText.text = $"{cost}g";

        // Grey out if can't afford
        if (btn) btn.interactable = GameManager.Instance.Gold >= cost;
    }

    // ── Button callbacks ──────────────────────────────
    // These are called by Unity UI buttons via OnClick in Inspector

    public void OnClickUpgradeA()
    {
        if (selectedTower == null) return;
        bool success = selectedTower.TryUpgrade(0);
        if (success) Refresh();
        else
        {
            // Flash "not enough gold"
            FloatingTextPool.Instance?.Spawn(
                panel.transform.position,
                "Not enough gold!", Color.red);
        }
    }

    public void OnClickUpgradeB()
    {
        if (selectedTower == null) return;
        bool success = selectedTower.TryUpgrade(1);
        if (success) Refresh();
        else
        {
            FloatingTextPool.Instance?.Spawn(
                panel.transform.position,
                "Not enough gold!", Color.red);
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

        // Clear reference AFTER destroying
        selectedTower = null;
        isOpen = false;
        panel.SetActive(false);
    }

    // Called when clicking empty space — add this to TowerPlacer.OnClick()
    public void CloseIfClickedElsewhere()
    {
        if (isOpen) Hide();
    }
}