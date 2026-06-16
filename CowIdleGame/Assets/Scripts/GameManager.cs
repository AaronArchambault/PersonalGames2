using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Config")]
    public CowEvolutionDatabase database;
    public float offlineEarningsMultiplier = 0.5f;  // earn 50% while away

    // ── Economy ──────────────────────────────────────────────────────────────
    public double Coins { get; private set; }
    public double CoinsPerSecond { get; private set; }

    public event Action<double> OnCoinsChanged;
    public event Action<double> OnCPSChanged;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Start()
    {
        ApplyOfflineEarnings();
        InvokeRepeating(nameof(TickIncome), 1f, 1f);
    }

    void OnApplicationPause(bool paused) { if (paused) Save(); }
    void OnApplicationQuit() => Save();

    // ── Income ────────────────────────────────────────────────────────────────
    void TickIncome()
    {
        if (CoinsPerSecond <= 0) return;
        float multiplier = PrestigeManager.Instance ? PrestigeManager.Instance.IncomeMultiplier : 1f;
        AddCoins(CoinsPerSecond * multiplier);
    }

    public void ResetCoins()
    {
        Coins = 0;
        OnCoinsChanged?.Invoke(Coins);
    }

    public void RecalculateCPS()
    {
        CoinsPerSecond = 0;
        foreach (var cow in FindObjectsOfType<CowEntity>())
            CoinsPerSecond += cow.Data.coinsPerSecond;
        OnCPSChanged?.Invoke(CoinsPerSecond);
    }

    // ── Coin helpers ──────────────────────────────────────────────────────────
    public void AddCoins(double amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    public bool SpendCoins(double amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }

    // ── Save / Load ───────────────────────────────────────────────────────────
    const string KEY_COINS = "coins";
    const string KEY_QUIT  = "quitTime";

    void Save()
    {
        PlayerPrefs.SetString(KEY_COINS, Coins.ToString("F2"));
        PlayerPrefs.SetString(KEY_QUIT, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.Save();
        GridManager.Instance?.SaveGrid();
    }

    void Load()
    {
        if (PlayerPrefs.HasKey(KEY_COINS))
            Coins = double.Parse(PlayerPrefs.GetString(KEY_COINS));
    }

    void ApplyOfflineEarnings()
    {
        if (!PlayerPrefs.HasKey(KEY_QUIT)) return;
        long binary = long.Parse(PlayerPrefs.GetString(KEY_QUIT));
        DateTime quitTime = DateTime.FromBinary(binary);
        double secondsAway = (DateTime.UtcNow - quitTime).TotalSeconds;
        double earned = CoinsPerSecond * secondsAway * offlineEarningsMultiplier;
        if (earned > 0)
        {
            AddCoins(earned);
            UIManager.Instance?.ShowOfflineEarnings(earned, secondsAway);
        }
    }
}






/*using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Config")]
    public CowEvolutionDatabase database;
    public float offlineEarningsMultiplier = 0.5f;  // earn 50% while away

    // ── Economy ──────────────────────────────────────────────────────────────
    public double Coins { get; private set; }
    public double CoinsPerSecond { get; private set; }

    public event Action<double> OnCoinsChanged;
    public event Action<double> OnCPSChanged;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Start()
    {
        ApplyOfflineEarnings();
        InvokeRepeating(nameof(TickIncome), 1f, 1f);
    }

    void OnApplicationPause(bool paused) { if (paused) Save(); }
    void OnApplicationQuit() => Save();

    // ── Income ────────────────────────────────────────────────────────────────
    void TickIncome()
    {
        if (CoinsPerSecond <= 0) return;
        AddCoins(CoinsPerSecond);
    }

    public void RecalculateCPS()
    {
        CoinsPerSecond = 0;
        foreach (var cow in FindObjectsOfType<CowEntity>())
            CoinsPerSecond += cow.Data.coinsPerSecond;
        OnCPSChanged?.Invoke(CoinsPerSecond);
    }

    // ── Coin helpers ──────────────────────────────────────────────────────────
    public void AddCoins(double amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    public bool SpendCoins(double amount)
    {
        if (Coins < amount) return false;
        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }

    // ── Save / Load ───────────────────────────────────────────────────────────
    const string KEY_COINS = "coins";
    const string KEY_QUIT  = "quitTime";

    void Save()
    {
        PlayerPrefs.SetString(KEY_COINS, Coins.ToString("F2"));
        PlayerPrefs.SetString(KEY_QUIT, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.Save();
        GridManager.Instance?.SaveGrid();
    }

    void Load()
    {
        if (PlayerPrefs.HasKey(KEY_COINS))
            Coins = double.Parse(PlayerPrefs.GetString(KEY_COINS));
    }

    void ApplyOfflineEarnings()
    {
        if (!PlayerPrefs.HasKey(KEY_QUIT)) return;
        long binary = long.Parse(PlayerPrefs.GetString(KEY_QUIT));
        DateTime quitTime = DateTime.FromBinary(binary);
        double secondsAway = (DateTime.UtcNow - quitTime).TotalSeconds;
        double earned = CoinsPerSecond * secondsAway * offlineEarningsMultiplier;
        if (earned > 0)
        {
            AddCoins(earned);
            UIManager.Instance?.ShowOfflineEarnings(earned, secondsAway);
        }
    }
}*/
