using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class PowerUpShopItem
{
    public string          powerUpId;
    public string          displayName;
    [TextArea(1,2)]
    public string          description;
    public int             cost;
    public Sprite          icon;
}

public class CatToyMarket : MonoBehaviour
{
    public static CatToyMarket Instance { get; private set; }

    [Header("Shop Settings")]
    public List<PowerUpShopItem> allPowerUps = new();
    public int  itemsOfferedPerWave = 3;

    [Header("UI")]
    public GameObject        marketPanel;
    public Transform         itemContainer;  // parent for item buttons
    public GameObject        itemButtonPrefab;
    public TextMeshProUGUI   timerText;
    public float             shopOpenDuration = 20f;

    private List<PowerUpShopItem> currentOffers = new();
    private float shopTimer = 0f;
    private bool  shopOpen  = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete += OpenShop;
    }

    void OnDisable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= OpenShop;
    }

    void Update()
    {
        if (!shopOpen) return;
        shopTimer -= Time.deltaTime;
        if (timerText) timerText.text = $"Shop closes in {Mathf.CeilToInt(shopTimer)}s";
        if (shopTimer <= 0) CloseShop();
    }

    public void OpenShop()
    {
        if (allPowerUps.Count == 0) return;

        // Pick random offers (no duplicates)
        currentOffers.Clear();
        var pool = new List<PowerUpShopItem>(allPowerUps);
        int count = Mathf.Min(itemsOfferedPerWave, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            currentOffers.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        BuildShopUI();
        marketPanel.SetActive(true);
        shopOpen  = true;
        shopTimer = shopOpenDuration;
        Time.timeScale = 0f; // pause game while shop is open
    }

    void BuildShopUI()
    {
        // Clear old items
        foreach (Transform child in itemContainer) Destroy(child.gameObject);

        foreach (var item in currentOffers)
        {
            var obj = Instantiate(itemButtonPrefab, itemContainer);
            var btn = obj.GetComponent<ShopItemButton>();
            btn?.Setup(item, this);
        }
    }

    public void BuyItem(PowerUpShopItem item)
    {
        if (!GameManager.Instance.SpendGold(item.cost)) return;
        PowerUpManager.Instance.ActivatePowerUp(item.powerUpId);
        FloatingTextPool.Instance?.Spawn(
            Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f)),
            $"{item.displayName} ACTIVATED!", Color.green);
        AudioManager.Instance?.Play("powerup_buy");
    }

    public void CloseShop()
    {
        shopOpen = false;
        marketPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnCloseButton() => CloseShop();
}

// Attach to each item button prefab in the shop
public class ShopItemButton : MonoBehaviour
{
    public Image            iconImage;
    public TextMeshProUGUI  nameText;
    public TextMeshProUGUI  descText;
    public TextMeshProUGUI  costText;
    public Button           buyButton;

    private PowerUpShopItem item;
    private CatToyMarket    market;

    public void Setup(PowerUpShopItem i, CatToyMarket m)
    {
        item   = i;
        market = m;
        if (iconImage) iconImage.sprite = i.icon;
        if (nameText)  nameText.text    = i.displayName;
        if (descText)  descText.text    = i.description;
        if (costText)  costText.text    = $"{i.cost}g";

        buyButton?.onClick.AddListener(() => market.BuyItem(item));
    }

    void Update()
    {
        if (buyButton)
            buyButton.interactable = GameManager.Instance.Gold >= item.cost;
    }
}