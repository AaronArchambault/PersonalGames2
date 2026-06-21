
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
 
[System.Serializable]
public class CatProfile
{
    public string catName;
    public string towerType;        // which tower class this cat is
    public string personality;      // "Grumpy", "Playful", "Lazy" etc
    public string catchphrase;
    public Sprite portrait;
    public int    totalKills;
    public int    level;            // 1-10, increases with kills
 
    // Personality-based stat bonuses
    public float damageBonus   => personality == "Aggressive" ? 0.15f : 0f;
    public float speedBonus    => personality == "Playful"    ? 0.1f  : 0f;
    public float incomeBonus   => personality == "Greedy"     ? 0.2f  : 0f;
 
    public string LevelTitle => level switch {
        1  => "Kitten",
        2  => "Curious Cat",
        3  => "House Cat",
        4  => "Alley Cat",
        5  => "Street Cat",
        6  => "Alpha Cat",
        7  => "Battle Cat",
        8  => "Legendary Cat",
        9  => "Ancient Cat",
        10 => "Cat God",
        _  => "Unknown"
    };
}
 
public class CatProfileCard : MonoBehaviour
{
    public static CatProfileCard Instance { get; private set; }
 
    public List<CatProfile> catRoster = new();
 
    [Header("UI — Card Panel")]
    public GameObject      cardPanel;
    public Image           portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI personalityText;
    public TextMeshProUGUI catchphraseText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI bonusText;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        if (cardPanel) cardPanel.SetActive(false);
    }
 
    public void ShowCard(string towerType)
    {
        var cat = catRoster.Find(c => c.towerType == towerType);
        if (cat == null || cardPanel == null) return;
 
        if (portraitImage)    portraitImage.sprite    = cat.portrait;
        if (nameText)         nameText.text           = cat.catName;
        if (levelText)        levelText.text          = $"Lv.{cat.level} {cat.LevelTitle}";
        if (personalityText)  personalityText.text    = cat.personality;
        if (catchphraseText)  catchphraseText.text    = $"\"{cat.catchphrase}\"";
        if (killsText)        killsText.text          = $"{cat.totalKills} enemies defeated";
 
        string bonuses = "";
        if (cat.damageBonus > 0)  bonuses += $"+{cat.damageBonus * 100:0}% dmg  ";
        if (cat.speedBonus > 0)   bonuses += $"+{cat.speedBonus * 100:0}% rate  ";
        if (cat.incomeBonus > 0)  bonuses += $"+{cat.incomeBonus * 100:0}% gold";
        if (bonusText) bonusText.text = bonuses;
 
        cardPanel.SetActive(true);
    }
 
    public void HideCard() { if (cardPanel) cardPanel.SetActive(false); }
 
    public void RegisterKill(string towerType)
    {
        var cat = catRoster.Find(c => c.towerType == towerType);
        if (cat == null) return;
        cat.totalKills++;
        // Level up every 50 kills, max level 10
        cat.level = Mathf.Clamp(1 + cat.totalKills / 50, 1, 10);
        SaveProfiles();
    }
 
    public void SaveProfiles()
    {
        for (int i = 0; i < catRoster.Count; i++)
        {
            PlayerPrefs.SetInt($"cat_{i}_kills", catRoster[i].totalKills);
            PlayerPrefs.SetInt($"cat_{i}_level", catRoster[i].level);
        }
        PlayerPrefs.Save();
    }
 
    public void LoadProfiles()
    {
        for (int i = 0; i < catRoster.Count; i++)
        {
            catRoster[i].totalKills = PlayerPrefs.GetInt($"cat_{i}_kills", 0);
            catRoster[i].level      = PlayerPrefs.GetInt($"cat_{i}_level", 1);
        }
    }
}