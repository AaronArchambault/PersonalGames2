using UnityEngine;
using System;

public class DailyChallengeManager : MonoBehaviour
{
    public static DailyChallengeManager Instance { get; private set; }

    [Header("Daily Challenge")]
    public int   startingGoldOverride = 100;  // limited starting gold
    public int   startingLivesOverride = 10;
    public int   dailySeed;                   // set from date

    public string[] allowedTowerTags;         // only these towers available today

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (!GameModeManager.Instance?.IsDailyChallenge == true) return;

        // Generate a seed from today's date
        DateTime today = DateTime.Now;
        dailySeed = today.Year * 10000 + today.Month * 100 + today.Day;
        UnityEngine.Random.InitState(dailySeed);

        // Apply overrides
        // (GameManager would need to support overriding start values)
        Debug.Log($"[DailyChallenge] Today's seed: {dailySeed}");
    }

    public int GetDailyScore()
    {
        int score = GameManager.Instance.Lives * 100;
        score += GameManager.Instance.Gold;
        score += GameManager.Instance.Wave * 50;
        return score;
    }

    public void SaveDailyScore()
    {
        string key = $"daily_{DateTime.Now:yyyyMMdd}";
        int current = PlayerPrefs.GetInt(key, 0);
        int newScore = GetDailyScore();
        if (newScore > current)
        {
            PlayerPrefs.SetInt(key, newScore);
            UIManager.Instance?.Announce($"DAILY SCORE: {newScore}!", Color.yellow);
        }
    }
}