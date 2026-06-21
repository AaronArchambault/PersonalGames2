
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
 
[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public Sprite icon;
    public bool   IsUnlocked => PlayerPrefs.GetInt($"ach_{id}", 0) == 1;
}
 
public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }
 
    public List<Achievement> achievements = new();
 
    [Header("Notification UI")]
    public GameObject      notificationPanel;
    public Image           notificationIcon;
    public TextMeshProUGUI notificationTitle;
    public TextMeshProUGUI notificationDesc;
 
    private Dictionary<string, Achievement> lookup = new();
 
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
 
        foreach (var a in achievements) lookup[a.id] = a;
        if (notificationPanel) notificationPanel.SetActive(false);
    }
 
    public void UnlockAchievement(string id)
    {
        if (!lookup.TryGetValue(id, out var ach)) return;
        if (ach.IsUnlocked) return;
 
        PlayerPrefs.SetInt($"ach_{id}", 1);
        PlayerPrefs.Save();
        StartCoroutine(ShowNotification(ach));
    }
 
    public void CheckKillAchievements(string towerType, int totalKills)
    {
        if (totalKills >= 100)  UnlockAchievement($"{towerType}_100_kills");
        if (totalKills >= 500)  UnlockAchievement($"{towerType}_500_kills");
        if (totalKills >= 1000) UnlockAchievement($"{towerType}_1000_kills");
    }
 
    IEnumerator ShowNotification(Achievement ach)
    {
        if (notificationPanel == null) yield break;
 
        if (notificationIcon)  notificationIcon.sprite = ach.icon;
        if (notificationTitle) notificationTitle.text  = $"Achievement: {ach.title}";
        if (notificationDesc)  notificationDesc.text   = ach.description;
 
        notificationPanel.SetActive(true);
 
        // Slide in from right
        var rt = notificationPanel.GetComponent<RectTransform>();
        if (rt != null)
        {
            Vector2 target = rt.anchoredPosition;
            Vector2 start  = target + Vector2.right * 400f;
            float t = 0;
            while (t < 0.3f)
            {
                rt.anchoredPosition = Vector2.Lerp(start, target, t / 0.3f);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            rt.anchoredPosition = target;
        }
 
        yield return new WaitForSecondsRealtime(3f);
 
        // Slide out
        if (rt != null)
        {
            Vector2 start = rt.anchoredPosition;
            Vector2 end   = start + Vector2.right * 400f;
            float t = 0;
            while (t < 0.2f)
            {
                rt.anchoredPosition = Vector2.Lerp(start, end, t / 0.2f);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
 
        notificationPanel.SetActive(false);
    }
}
 