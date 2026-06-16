using UnityEngine;
using TMPro;

public class ShopController : MonoBehaviour
{
    [Header("Buy Cow Button")]
    public double buyCowBaseCost = 10;
    public double costScaleFactor = 1.15;   // price increases each purchase
    public TMP_Text buyCowPriceLabel;

    int _purchaseCount = 0;

    void Start() => RefreshPriceLabel();

    public void OnBuyCowPressed()
    {
        double cost = GetCurrentCost();
        if (!GameManager.Instance.SpendCoins(cost))
        {
            UIManager.Instance?.ShowMessage("Not enough coins!");
            return;
        }

        if (!GridManager.Instance.HasEmptyCell())
        {
            UIManager.Instance?.ShowMessage("No empty spaces!");
            // Refund
            GameManager.Instance.AddCoins(cost);
            return;
        }

        GridManager.Instance.SpawnCow(0);   // spawn tier-0 cow
        _purchaseCount++;
        RefreshPriceLabel();
    }

    double GetCurrentCost() =>
        Mathf.RoundToInt((float)(buyCowBaseCost * Mathf.Pow((float)costScaleFactor, _purchaseCount)));

    void RefreshPriceLabel()
    {
        if (buyCowPriceLabel)
            buyCowPriceLabel.text = FormatCoins(GetCurrentCost());
    }

    public static string FormatCoins(double v)
    {
        if (v >= 1_000_000_000) return $"{v / 1_000_000_000:F1}B";
        if (v >= 1_000_000)     return $"{v / 1_000_000:F1}M";
        if (v >= 1_000)         return $"{v / 1_000:F1}K";
        return Mathf.RoundToInt((float)v).ToString();
    }
}
