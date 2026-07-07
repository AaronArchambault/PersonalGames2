
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class LevelButtonUI : MonoBehaviour
{
    [Header("UI References — assign in prefab")]
    public Image           themeIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI difficultyText;
    public Image[]         starImages;
    public Sprite          starFull;
    public Sprite          starEmpty;
    public Image           lockOverlay;
    public Button          button;
 
    private LevelSelectManager.LevelData level;
    private LevelSelectManager           manager;
 
    public void Setup(LevelSelectManager.LevelData data, LevelSelectManager mgr)
    {
        level   = data;
        manager = mgr;
 
        // Theme icon and colour
        if (themeIcon)
        {
            if (data.themeIcon) themeIcon.sprite = data.themeIcon;
            themeIcon.color = data.themeColor;
        }
 
        if (nameText)       nameText.text       = data.displayName;
        if (difficultyText) difficultyText.text = data.difficulty switch
        {
            1 => "Easy",
            2 => "Medium",
            3 => "Hard",
            _ => ""
        };
 
        // Stars
        int stars = PlayerPrefs.GetInt($"stars_{data.sceneName}", 0);
        for (int i = 0; i < starImages.Length; i++)
            if (starImages[i])
                starImages[i].sprite = i < stars ? starFull : starEmpty;
 
        // Lock overlay
        if (lockOverlay) lockOverlay.gameObject.SetActive(!data.isUnlocked);
 
        // Button
        if (button)
        {
            button.interactable = data.isUnlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => manager.SelectLevel(level));
        }
 
        // Add animated button if missing
        if (GetComponent<AnimatedButton>() == null)
            gameObject.AddComponent<AnimatedButton>();
    }
}