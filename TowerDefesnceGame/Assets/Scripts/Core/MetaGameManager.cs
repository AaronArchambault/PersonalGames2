
using UnityEngine;
using System.Collections.Generic;
 
public class MetagameManager : MonoBehaviour
{
    public static MetagameManager Instance { get; private set; }
 
    public int TotalXP        { get; private set; }
    public int TotalKills     { get; private set; }
    public int GamesPlayed    { get; private set; }
 
    // Which towers are unlocked (by their class name string)
    private HashSet<string> unlockedTowers = new();
    private Dictionary<string, int> killsByTowerType = new();
 
    [Header("Unlock Thresholds (XP needed)")]
    public List<TowerUnlockEntry> towerUnlocks = new();
 
    [System.Serializable]
    public class TowerUnlockEntry
    {
        public string towerClassName;
        public string displayName;
        public int    xpRequired;
        public bool   IsUnlocked => MetagameManager.Instance.IsUnlocked(towerClassName);
    }
 
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
        LoadProgress();
    }
 
    public bool IsUnlocked(string towerClass)
    {
        return unlockedTowers.Contains(towerClass);
    }
 
    public void AddXP(int amount)
    {
        TotalXP += amount;
        CheckUnlocks();
        SaveProgress();
    }
 
    public void AddKill(string towerType)
    {
        TotalKills++;
        if (!killsByTowerType.ContainsKey(towerType)) killsByTowerType[towerType] = 0;
        killsByTowerType[towerType]++;
        AchievementSystem.Instance?.CheckKillAchievements(towerType, killsByTowerType[towerType]);
    }
 
    void CheckUnlocks()
    {
        foreach (var entry in towerUnlocks)
        {
            if (!unlockedTowers.Contains(entry.towerClassName) && TotalXP >= entry.xpRequired)
            {
                unlockedTowers.Add(entry.towerClassName);
                UIManager.Instance?.Announce($"UNLOCKED: {entry.displayName}!", Color.yellow);
                AchievementSystem.Instance?.UnlockAchievement($"unlock_{entry.towerClassName}");
            }
        }
    }
 
    public int GetKillsWithTower(string towerType) =>
        killsByTowerType.TryGetValue(towerType, out int v) ? v : 0;
 
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("meta_xp",     TotalXP);
        PlayerPrefs.SetInt("meta_kills",  TotalKills);
        PlayerPrefs.SetInt("meta_games",  GamesPlayed);
        foreach (var t in unlockedTowers)
            PlayerPrefs.SetInt($"unlock_{t}", 1);
        PlayerPrefs.Save();
    }
 
    void LoadProgress()
    {
        TotalXP     = PlayerPrefs.GetInt("meta_xp",    0);
        TotalKills  = PlayerPrefs.GetInt("meta_kills", 0);
        GamesPlayed = PlayerPrefs.GetInt("meta_games", 0);
 
        foreach (var entry in towerUnlocks)
            if (PlayerPrefs.GetInt($"unlock_{entry.towerClassName}", 0) == 1)
                unlockedTowers.Add(entry.towerClassName);
    }
}