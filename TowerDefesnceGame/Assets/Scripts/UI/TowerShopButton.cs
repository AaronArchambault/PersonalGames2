using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerShopButton : MonoBehaviour
{
    public GameObject towerPrefab;
    public int        cost;
    public string     towerName;

    [Header("UI Refs")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Button          button;

    void Start()
    {
        if (nameText) nameText.text = towerName;
        if (costText) costText.text = $"{cost}g";
    }

    void Update()
    {
        // Grey out if can't afford
        if (button) button.interactable = GameManager.Instance.Gold >= cost;
    }

    public void OnClick() =>
        TowerPlacer.Instance.BeginPlacement(towerPrefab, cost);
}