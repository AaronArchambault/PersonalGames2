
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
 
public class LevelSelectManager : MonoBehaviour
{
    public static LevelSelectManager Instance { get; private set; }
 
    [Header("UI")]
    public Transform  themeContainer;
    public Transform  levelContainer;
    public GameObject themeButtonPrefab;
    public GameObject levelButtonPrefab;
 
    [Header("Level Info Panel")]
    public GameObject      infoPanel;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI startingGoldText;
    public TextMeshProUGUI wavesText;
    public TextMeshProUGUI descriptionText;
    public Image[]         starImages;
    public Sprite          starFull;
    public Sprite          starEmpty;
    public Button          playButton;
 
    [Header("All Levels")]
    public List<LevelData> levels = new();
 
    [System.Serializable]
    public class LevelData
    {
        public string sceneName;     // must match Unity scene name exactly
        public string displayName;
        public string themeName;
        public string description;
        public int    difficulty;    // 1=Easy 2=Medium 3=Hard
        public int    startingGold;
        public int    waveCount;
        public bool   isUnlocked;
        public Sprite themeIcon;
        public Color  themeColor;
        // Just the filename without path or extension e.g. "LivingRoom_1"
        // WaveManager prepends "Levels/" and appends ".json"
        public string waveJsonFile;
    }
 
    private LevelData selectedLevel;
    private string    currentThemeFilter = "";
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
 
    void Start()
    {
        LoadUnlockStates();
        BuildThemeButtons();
        ShowAllLevels();
        if (infoPanel) infoPanel.SetActive(false);
    }
 
    void LoadUnlockStates()
    {
        foreach (var level in levels)
        {
            // Easy levels always unlocked
            if (level.difficulty == 1)
            {
                level.isUnlocked = true;
                continue;
            }
 
            // Medium: needs 1+ stars on the Easy version
            if (level.difficulty == 2)
            {
                string easyScene = level.sceneName.Replace("_2", "_1");
                int stars = PlayerPrefs.GetInt($"stars_{easyScene}", 0);
                level.isUnlocked = stars >= 1;
                continue;
            }
 
            // Hard: needs 2+ stars on the Medium version
            if (level.difficulty == 3)
            {
                string medScene = level.sceneName.Replace("_3", "_2");
                int stars = PlayerPrefs.GetInt($"stars_{medScene}", 0);
                level.isUnlocked = stars >= 2;
            }
        }
    }
 
    void BuildThemeButtons()
    {
        if (themeContainer == null || themeButtonPrefab == null) return;
        foreach (Transform child in themeContainer) Destroy(child.gameObject);
 
        string[] themes =
        {
            "All", "LivingRoom", "Kitchen", "Garden",
            "Rooftop", "Basement", "VetOffice"
        };
 
        foreach (var theme in themes)
        {
            var obj = Instantiate(themeButtonPrefab, themeContainer);
            var btn = obj.GetComponent<Button>();
            var lbl = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl) lbl.text = theme == "All" ? "All" : FormatThemeName(theme);
            string t = theme;
            btn?.onClick.AddListener(() => FilterByTheme(t));
        }
    }
 
    void FilterByTheme(string theme)
    {
        currentThemeFilter = theme == "All" ? "" : theme;
        ShowAllLevels();
    }
 
    void ShowAllLevels()
    {
        if (levelContainer == null || levelButtonPrefab == null) return;
        foreach (Transform child in levelContainer) Destroy(child.gameObject);
 
        foreach (var level in levels)
        {
            if (!string.IsNullOrEmpty(currentThemeFilter) &&
                level.themeName != currentThemeFilter) continue;
 
            var obj = Instantiate(levelButtonPrefab, levelContainer);
            var btn = obj.GetComponent<LevelButtonUI>();
            btn?.Setup(level, this);
        }
    }
 
    public void SelectLevel(LevelData level)
    {
        selectedLevel = level;
        if (infoPanel) infoPanel.SetActive(true);
 
        if (levelNameText)    levelNameText.text    = level.displayName;
        if (difficultyText)   difficultyText.text   = DifficultyString(level.difficulty);
        if (startingGoldText) startingGoldText.text = $"Starting Gold: {level.startingGold}g";
        if (wavesText)        wavesText.text        = $"Waves: {level.waveCount}";
        if (descriptionText)  descriptionText.text  = level.description;
        if (playButton)       playButton.interactable = level.isUnlocked;
 
        int stars = PlayerPrefs.GetInt($"stars_{level.sceneName}", 0);
        for (int i = 0; i < starImages.Length; i++)
            if (starImages[i])
                starImages[i].sprite = i < stars ? starFull : starEmpty;
    }
 
    public void OnPlayButton()
    {
        if (selectedLevel == null || !selectedLevel.isUnlocked) return;
 
        // Store for WaveManager and GameManager to read on scene load
        PlayerPrefs.SetString("current_wave_file",      selectedLevel.waveJsonFile);
        PlayerPrefs.SetInt   ("current_starting_gold",  selectedLevel.startingGold);
        PlayerPrefs.SetString("current_theme",          selectedLevel.themeName);
        PlayerPrefs.Save();
 
        SceneManager.LoadScene(selectedLevel.sceneName);
    }
 
    public void OnBackButton() => SceneManager.LoadScene("MainMenu");
 
    string DifficultyString(int d) => d switch
    {
        1 => "★ Easy",
        2 => "★★ Medium",
        3 => "★★★ Hard",
        _ => ""
    };
 
    string FormatThemeName(string t) => t switch
    {
        "LivingRoom" => "Living Room",
        "VetOffice"  => "Vet Office",
        _            => t
    };
}
 
 