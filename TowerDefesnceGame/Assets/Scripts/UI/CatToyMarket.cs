
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
 
[System.Serializable]
public class PowerUpShopItem
{
    public string powerUpId;
    public string displayName;
    [TextArea(1, 2)]
    public string description;
    public int    cost;
    public Sprite icon;
}
 
public class CatToyMarket : MonoBehaviour
{
    public static CatToyMarket Instance { get; private set; }
 
    [Header("Power-Up Items")]
    public List<PowerUpShopItem> allPowerUps = new();
 
    [Header("Shop Settings")]
    public int   itemsOfferedPerWave  = 3;
    public float shopOpenDuration     = 20f;
 
    [Header("UI References")]
    public GameObject      marketPanel;
    public Transform       itemContainer;      // parent for buttons (Horizontal Layout Group)
    public GameObject      itemButtonPrefab;   // ShopItemButton prefab
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI titleText;
    public Button          closeButton;
 
    private List<PowerUpShopItem> currentOffers = new();
    private float shopTimer = 0f;
    private bool  shopOpen  = false;
    private Coroutine timerCoroutine;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
 
        if (marketPanel) marketPanel.SetActive(false);
        if (closeButton) closeButton.onClick.AddListener(CloseShop);
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
 
    // ── Shop Open / Close ─────────────────────────────────────
 
    public void OpenShop()
    {
        if (allPowerUps == null || allPowerUps.Count == 0)
        {
            Debug.LogWarning("[CatToyMarket] No power-ups assigned in allPowerUps list!");
            return;
        }
 
        if (itemButtonPrefab == null)
        {
            Debug.LogWarning("[CatToyMarket] itemButtonPrefab is not assigned!");
            return;
        }
 
        // Pick random offers without duplicates
        currentOffers.Clear();
        var pool  = new List<PowerUpShopItem>(allPowerUps);
        int count = Mathf.Min(itemsOfferedPerWave, pool.Count);
 
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            currentOffers.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
 
        BuildShopUI();
 
        if (marketPanel) marketPanel.SetActive(true);
        if (titleText)   titleText.text = "Cat Toy Market";
 
        shopOpen  = true;
        shopTimer = shopOpenDuration;
 
        // Pause the game while shop is open
        Time.timeScale = 0f;
 
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(ShopTimerRoutine());
    }
 
    void BuildShopUI()
    {
        if (itemContainer == null)
        {
            Debug.LogWarning("[CatToyMarket] itemContainer is not assigned!");
            return;
        }
 
        // Clear old buttons
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);
 
        // Create one button per offer
        foreach (var item in currentOffers)
        {
            GameObject btnObj = Instantiate(itemButtonPrefab, itemContainer);
            var btn = btnObj.GetComponent<ShopItemButton>();
 
            if (btn != null)
            {
                btn.Setup(item, this);
            }
            else
            {
                Debug.LogWarning("[CatToyMarket] itemButtonPrefab does not have a ShopItemButton component!");
            }
        }
    }
 
    IEnumerator ShopTimerRoutine()
    {
        // Use unscaled time because game is paused (timeScale = 0)
        float elapsed = 0f;
        while (elapsed < shopOpenDuration && shopOpen)
        {
            elapsed += Time.unscaledDeltaTime;
            float remaining = shopOpenDuration - elapsed;
            if (timerText)
                timerText.text = $"Shop closes in {Mathf.CeilToInt(remaining)}s";
            yield return null;
        }
        if (shopOpen) CloseShop();
    }
 
    public void BuyItem(PowerUpShopItem item)
    {
        if (item == null) return;
 
        if (GameManager.Instance == null || !GameManager.Instance.SpendGold(item.cost))
        {
            FloatingTextPool.Instance?.Spawn(
                marketPanel != null ? marketPanel.transform.position : Vector3.zero,
                "Not enough gold!", Color.red);
            return;
        }
 
        PowerUpManager.Instance?.ActivatePowerUp(item.powerUpId);
 
        FloatingTextPool.Instance?.Spawn(
            Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f)),
            $"{item.displayName} activated!", Color.green);
 
        AudioManager.Instance?.Play("powerup_buy");
 
        // Refresh button states after purchase
        RefreshButtonStates();
    }
 
    void RefreshButtonStates()
    {
        if (itemContainer == null) return;
        foreach (Transform child in itemContainer)
        {
            var btn = child.GetComponent<ShopItemButton>();
            btn?.RefreshAffordable();
        }
    }
 
    public void CloseShop()
    {
        shopOpen = false;
        if (marketPanel) marketPanel.SetActive(false);
        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }
 
        // Resume game
        Time.timeScale = 1f;
    }
 
    public void OnCloseButton() => CloseShop();
}