using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerUpgradeUI : MonoBehaviour
{
    public static TowerUpgradeUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;
    public TextMeshProUGUI towerNameText;

    [Header("Path A")]
    public TextMeshProUGUI pathANameText;
    public TextMeshProUGUI pathACostText;
    public Button pathAButton;

    [Header("Path B")]
    public TextMeshProUGUI pathBNameText;
    public TextMeshProUGUI pathBCostText;
    public Button pathBButton;

    [Header("Sell")]
    public TextMeshProUGUI sellValueText;
    public Button sellButton;

    private Tower selectedTower;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        panel.SetActive(false);
    }

    public void Show(Tower tower)
    {
        selectedTower = tower;
        panel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        if (selectedTower) selectedTower.SetSelected(false);
        selectedTower = null;
        panel.SetActive(false);
    }

    void Refresh()
    {
        if (!selectedTower) return;
        towerNameText.text = selectedTower.upgradeData?.towerName ?? selectedTower.name;

        RefreshPath(0, selectedTower.pathALevel, pathANameText, pathACostText, pathAButton);
        RefreshPath(1, selectedTower.pathBLevel, pathBNameText, pathBCostText, pathBButton);

      /*  int sell = Mathf.RoundToInt(selectedTower.cost * 0.6f);
        sellValueText.text = $"Sell  +{sell}g";*/
        // In TowerUpgradeUI.Refresh():
int sell = selectedTower.GetSellValue();
sellValueText.text = $"Sell +{sell}g  ({selectedTower.killCount} kills)";
    }

    void RefreshPath(int path, int level, TextMeshProUGUI nameText, TextMeshProUGUI costText, Button btn)
    {
        bool maxed = level >= 3;
        nameText.text = maxed ? "MAX" :
            (path == 0 ? selectedTower.upgradeData?.pathA[level].upgradeName
                       : selectedTower.upgradeData?.pathB[level].upgradeName) ?? "Upgrade";
        int cost = selectedTower.GetUpgradeCost(path);
        costText.text = maxed ? "" : $"{cost}g";
        btn.interactable = !maxed && GameManager.Instance.Gold >= cost;
    }

    public void OnClickUpgradeA() { if (selectedTower.TryUpgrade(0)) Refresh(); }
    public void OnClickUpgradeB() { if (selectedTower.TryUpgrade(1)) Refresh(); }


public void OnClickSell()
{
    int refund = selectedTower.GetSellValue();
    GameManager.Instance.EarnGold(refund);
    FloatingTextPool.Instance?.Spawn(
        selectedTower.transform.position + Vector3.up,
        $"+{refund}g", Color.yellow);
    Destroy(selectedTower.gameObject);
    Hide();
}

}


 /*   public void OnClickSell()
    {
            public int GetSellValue()
{
    int baseValue = Mathf.RoundToInt(cost * 0.6f);
    int killBonus = killCount * 2; // 2g per kill
    return baseValue + killBonus;

        int refund = Mathf.RoundToInt(selectedTower.cost * 0.6f);
        GameManager.Instance.EarnGold(refund);
        FloatingTextPool.Instance?.Spawn(selectedTower.transform.position + Vector3.up,
            $"+{refund}g", Color.yellow);
        Destroy(selectedTower.gameObject);
        Hide();
    }
}*/