
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class ShopItemButton : MonoBehaviour
{
    [Header("UI References — assign in prefab")]
    public Image           iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI costText;
    public Button          buyButton;
 
    private PowerUpShopItem item;
    private CatToyMarket    market;
 
    public void Setup(PowerUpShopItem powerUp, CatToyMarket marketRef)
    {
        item   = powerUp;
        market = marketRef;
 
        // Populate all UI fields
        if (iconImage)
        {
            iconImage.sprite  = powerUp.icon;
            iconImage.enabled = powerUp.icon != null;
        }
 
        if (nameText) nameText.text = powerUp.displayName;
        if (descText) descText.text = powerUp.description;
        if (costText) costText.text = $"{powerUp.cost}g";
 
        // Wire buy button
        if (buyButton)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
 
        // Add animated button if not already there
        if (buyButton != null && buyButton.GetComponent<AnimatedButton>() == null)
            buyButton.gameObject.AddComponent<AnimatedButton>();
 
        RefreshAffordable();
    }
 
    void OnBuyClicked()
    {
        if (item == null || market == null) return;
        market.BuyItem(item);
    }
 
    public void RefreshAffordable()
    {
        if (buyButton == null || item == null || GameManager.Instance == null) return;
        buyButton.interactable = GameManager.Instance.Gold >= item.cost;
 
        // Grey out cost text if can't afford
        if (costText)
            costText.color = GameManager.Instance.Gold >= item.cost
                ? Color.white
                : new Color(1f, 0.4f, 0.4f);
    }
}