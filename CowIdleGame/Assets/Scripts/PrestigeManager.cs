using System;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// PrestigeManager
//
// How prestige works in this game:
//   - Player must reach a minimum tier cow on the board (e.g. tier 5)
//   - Prestiging wipes all cows and coins
//   - Awards "Cowrium" (prestige currency) based on progress
//   - Each Cowrium point permanently boosts all coin income by a %
//   - Prestige count also unlocks cosmetic titles and future features
// ─────────────────────────────────────────────────────────────────────────────
public class PrestigeManager : MonoBehaviour
{
    public static PrestigeManager Instance { get; private set; }

    [Header("Requirements")]
    [Tooltip("Player must have at least one cow of this tier or higher to prestige")]
    public int minimumTierToPrestige = 5;

    [Header("Cowrium Rewards")]
    [Tooltip("Base Cowrium earned on first prestige")]
    public int baseCowriumReward = 1;
    [Tooltip("Each prestige after the first gives more Cowrium")]
    public float cowriumScalingPerPrestige = 0.5f;

    [Header("Income Multiplier")]
    [Tooltip("Each Cowrium point adds this % to all coin income")]
    public float incomeBoostPerCowrium = 0.10f;   // 10% per point

    // ── State ─────────────────────────────────────────────────────────────────
    public int    PrestigeCount  { get; private set; }
    public int    Cowrium        { get; private set; }
    public float  IncomeMultiplier => 1f + (Cowrium * incomeBoostPerCowrium);

    public event Action OnPrestiged;
    public event Action OnCowriumChanged;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => Load();

    // ── Prestige logic ─────────────────────────────────────────────────────────
    public bool CanPrestige()
    {
        foreach (var cow in FindObjectsOfType<CowEntity>())
            if (cow.Data.tier >= minimumTierToPrestige) return true;
        return false;
    }

    // Returns how much Cowrium this prestige would award (shown in UI before confirming)
    public int PreviewCowriumReward()
    {
        return Mathf.Max(1, Mathf.RoundToInt(
            baseCowriumReward + PrestigeCount * cowriumScalingPerPrestige));
    }

    public void ExecutePrestige()
    {
        if (!CanPrestige()) return;

        int reward = PreviewCowriumReward();
        Cowrium       += reward;
        PrestigeCount += 1;

        // Wipe board
        foreach (var cow in FindObjectsOfType<CowEntity>())
            Destroy(cow.gameObject);

        // Clear all grid cells
        GridManager.Instance.ClearAllCells();

        // Reset coins (keep Cowrium — that's the whole point)
        GameManager.Instance.ResetCoins();
        GameManager.Instance.RecalculateCPS();

        Save();
        OnCowriumChanged?.Invoke();
        OnPrestiged?.Invoke();

        UIManager.Instance?.ShowPrestigeResult(reward, PrestigeCount, IncomeMultiplier);
    }

    // ── Prestige titles (cosmetic) ─────────────────────────────────────────────
    static readonly string[] Titles =
    {
        "Farmer",           // 0
        "Rancher",          // 1
        "Cow Whisperer",    // 2
        "Moo Master",       // 3
        "Bovine Overlord",  // 4
        "Cosmic Herder",    // 5+
    };

    public string GetTitle() =>
        Titles[Mathf.Min(PrestigeCount, Titles.Length - 1)];

    // ── Save / Load ───────────────────────────────────────────────────────────
    const string KEY_COUNT   = "prestigeCount";
    const string KEY_COWRIUM = "cowrium";

    void Save()
    {
        PlayerPrefs.SetInt(KEY_COUNT,   PrestigeCount);
        PlayerPrefs.SetInt(KEY_COWRIUM, Cowrium);
        PlayerPrefs.Save();
    }

    void Load()
    {
        PrestigeCount = PlayerPrefs.GetInt(KEY_COUNT,   0);
        Cowrium       = PlayerPrefs.GetInt(KEY_COWRIUM, 0);
        OnCowriumChanged?.Invoke();
    }
}
