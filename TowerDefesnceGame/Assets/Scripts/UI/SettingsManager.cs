using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer audioMixer; // drag GameAudioMixer here

    [Header("Audio UI")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteToggle;
    public TextMeshProUGUI masterVolumeLabel;
    public TextMeshProUGUI musicVolumeLabel;
    public TextMeshProUGUI sfxVolumeLabel;

    [Header("Graphics UI")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public Slider brightnessSlider;
    public TextMeshProUGUI brightnessLabel;

    [Header("Accessibility UI")]
    public Toggle colorblindModeToggle;
    public TMP_Dropdown colorblindTypeDropdown;
    public Toggle reducedMotionToggle;
    public Toggle largeUIToggle;
    public Slider uiScaleSlider;
    public TextMeshProUGUI uiScaleLabel;
    public Toggle screenFlashToggle;
    public Toggle highContrastToggle;
    public Toggle dyslexiaFontToggle;
    public Slider gameSpeedSlider;
    public TextMeshProUGUI gameSpeedLabel;

    // Targets that accessibility affects
    [Header("Accessibility Targets")]
    public Canvas mainCanvas;         // for UI scale
    public GameObject[] uiRoots;      // panels to scale up for Large UI

    // PlayerPrefs keys
    const string K_MASTER    = "vol_master";
    const string K_MUSIC     = "vol_music";
    const string K_SFX       = "vol_sfx";
    const string K_MUTE      = "mute";
    const string K_QUALITY   = "quality";
    const string K_FULLSCR   = "fullscreen";
    const string K_VSYNC     = "vsync";
    const string K_BRIGHT    = "brightness";
    const string K_CB_MODE   = "cb_mode";
    const string K_CB_TYPE   = "cb_type";
    const string K_REDMOTION = "reduced_motion";
    const string K_LARGE_UI  = "large_ui";
    const string K_UI_SCALE  = "ui_scale";
    const string K_FLASH     = "screen_flash";
    const string K_CONTRAST  = "high_contrast";
    const string K_DYSLEXIA  = "dyslexia_font";
    const string K_GAMESPD   = "game_speed";
    const string K_RES_INDEX = "resolution_index";

   void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Listen for any scene load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    else
    {
        Destroy(gameObject);
        return;
    }
}
    void Start()
    {
        PopulateDropdowns();
        LoadAllSettings();
    }

    void OnDestroy()
{
    SceneManager.sceneLoaded -= OnSceneLoaded;
}

void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // Short delay so all scene objects have time to initialize
    StartCoroutine(ReapplyAfterLoad());
}

System.Collections.IEnumerator ReapplyAfterLoad()
{
    // Wait two frames for scene objects to run their Awake/Start
    yield return null;
    yield return null;

    ReapplyAudio();
    ReapplyGraphics();
    ReapplyAccessibility();

    Debug.Log($"[SettingsManager] Settings reapplied for scene: " +
              $"{SceneManager.GetActiveScene().name}");
}

void ReapplyAudio()
{
    float master = PlayerPrefs.GetFloat("vol_master", 1f);
    float music  = PlayerPrefs.GetFloat("vol_music",  0.8f);
    float sfx    = PlayerPrefs.GetFloat("vol_sfx",    1f);
    bool  mute   = PlayerPrefs.GetInt("mute", 0) == 1;
    ApplyAudio(master, music, sfx, mute);
}

void ReapplyGraphics()
{
    int   quality    = PlayerPrefs.GetInt("quality", QualitySettings.GetQualityLevel());
    bool  fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;
    bool  vsync      = PlayerPrefs.GetInt("vsync", 1) == 1;
    float brightness = PlayerPrefs.GetFloat("brightness", 1f);

    QualitySettings.SetQualityLevel(quality, true);
    Screen.fullScreen = fullscreen;
    QualitySettings.vSyncCount = vsync ? 1 : 0;

    // Brightness — find the overlay in the new scene or use persistent one
    StartCoroutine(ApplyBrightnessNextFrame(brightness));
}

void ReapplyAccessibility()
{
    bool  cbMode    = PlayerPrefs.GetInt("cb_mode", 0) == 1;
    int   cbType    = PlayerPrefs.GetInt("cb_type", 0);
    bool  redMotion = PlayerPrefs.GetInt("reduced_motion", 0) == 1;
    bool  largeUI   = PlayerPrefs.GetInt("large_ui", 0) == 1;
    float uiScale   = PlayerPrefs.GetFloat("ui_scale", 1f);
    bool  contrast  = PlayerPrefs.GetInt("high_contrast", 0) == 1;
    bool  dyslexia  = PlayerPrefs.GetInt("dyslexia_font", 0) == 1;
    float gameSpd   = PlayerPrefs.GetFloat("game_speed", 1f);

    // Update static values
    AccessibilitySettings.ColorblindMode = cbMode;
    AccessibilitySettings.ColorblindType = cbType;
    AccessibilitySettings.ReducedMotion  = redMotion;
    AccessibilitySettings.LargeUI        = largeUI;
    AccessibilitySettings.UIScale        = uiScale;
    AccessibilitySettings.HighContrast   = contrast;
    AccessibilitySettings.DyslexiaFont   = dyslexia;
    AccessibilitySettings.GameSpeed      = gameSpd;

    // Camera shake — find it in the new scene
    if (CameraShake.Instance != null)
        CameraShake.Instance.enabled = !redMotion;

    // Game speed — only apply in gameplay scene, not menu
    string sceneName = SceneManager.GetActiveScene().name;
    if (sceneName != "MainMenu")
        Time.timeScale = gameSpd;
    else
        Time.timeScale = 1f;

    // Colorblind overlay — works if using PersistentOverlays
    ColorblindOverlay.Instance?.Apply(cbMode, cbType);

    // Dyslexia font — reapply to all text in new scene
    if (dyslexia)
        DyslexiaFontApplier.Instance?.Apply(true);

    // UI scale — find canvas in new scene
    StartCoroutine(ReapplyUIScaleNextFrame(uiScale, largeUI));
}

System.Collections.IEnumerator ReapplyUIScaleNextFrame(float uiScale, bool largeUI)
{
    yield return null;
    // Find the game scene's canvas
    var allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
    foreach (var canvas in allCanvases)
    {
        // Skip the persistent overlays canvas
        if (canvas.gameObject.name == "PersistentOverlays") continue;
        var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler == null) continue;
        if (largeUI)
            scaler.referenceResolution = new Vector2(1280f, 720f);
        else
            scaler.referenceResolution = new Vector2(1920f * uiScale, 1080f * uiScale);
    }
}
    // ── POPULATE DROPDOWNS ────────────────────────────

    void PopulateDropdowns()
    {
        // Quality
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(
                new System.Collections.Generic.List<string>(QualitySettings.names));
        }

        // Resolution
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();
            var resolutions = Screen.resolutions;
            int savedIndex = PlayerPrefs.GetInt(K_RES_INDEX, -1);
            int currentIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                options.Add($"{resolutions[i].width} x {resolutions[i].height} " +
                            $"@ {Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value)}Hz");
                if (savedIndex == -1 &&
                    resolutions[i].width  == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                    currentIndex = i;
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = savedIndex >= 0 ? savedIndex : currentIndex;
            resolutionDropdown.RefreshShownValue();
        }

        // Colorblind type
        if (colorblindTypeDropdown != null)
        {
            colorblindTypeDropdown.ClearOptions();
            colorblindTypeDropdown.AddOptions(
                new System.Collections.Generic.List<string>
                { "Deuteranopia (Red-Green)", "Protanopia (Red)", "Tritanopia (Blue-Yellow)" });
        }
    }

    // ── LOAD ALL ──────────────────────────────────────

    void LoadAllSettings()
    {
        LoadAudio();
        LoadGraphics();
        LoadAccessibility();
    }

    void LoadAudio()
    {
        float master = PlayerPrefs.GetFloat(K_MASTER, 1f);
        float music  = PlayerPrefs.GetFloat(K_MUSIC,  0.8f);
        float sfx    = PlayerPrefs.GetFloat(K_SFX,    1f);
        bool  mute   = PlayerPrefs.GetInt(K_MUTE, 0) == 1;

        // Set sliders without triggering callbacks
        SetSliderSilent(masterVolumeSlider, master, OnMasterVolumeChanged);
        SetSliderSilent(musicVolumeSlider,  music,  OnMusicVolumeChanged);
        SetSliderSilent(sfxVolumeSlider,    sfx,    OnSFXVolumeChanged);
        SetToggleSilent(muteToggle, mute, OnMuteToggled);

        UpdateVolumeLabels(master, music, sfx);
        ApplyAudio(master, music, sfx, mute);
    }

    void LoadGraphics()
    {
        int   quality    = PlayerPrefs.GetInt(K_QUALITY, QualitySettings.GetQualityLevel());
        bool  fullscreen = PlayerPrefs.GetInt(K_FULLSCR, Screen.fullScreen ? 1 : 0) == 1;
        bool  vsync      = PlayerPrefs.GetInt(K_VSYNC, 1) == 1;
        float brightness = PlayerPrefs.GetFloat(K_BRIGHT, 1f);

        SetDropdownSilent(qualityDropdown,    quality,    OnQualityChanged);
        SetToggleSilent(fullscreenToggle,  fullscreen, OnFullscreenToggled);
        SetToggleSilent(vsyncToggle,       vsync,      OnVSyncToggled);
        SetSliderSilent(brightnessSlider,  brightness, OnBrightnessChanged);

        if (brightnessLabel)
            brightnessLabel.text = $"{Mathf.RoundToInt(brightness * 100)}%";

        ApplyGraphics(quality, fullscreen, vsync, brightness);
    }

    void LoadAccessibility()
    {
        bool  cbMode    = PlayerPrefs.GetInt(K_CB_MODE, 0) == 1;
        int   cbType    = PlayerPrefs.GetInt(K_CB_TYPE, 0);
        bool  redMotion = PlayerPrefs.GetInt(K_REDMOTION, 0) == 1;
        bool  largeUI   = PlayerPrefs.GetInt(K_LARGE_UI, 0) == 1;
        float uiScale   = PlayerPrefs.GetFloat(K_UI_SCALE, 1f);
        bool  flash     = PlayerPrefs.GetInt(K_FLASH, 1) == 1;
        bool  contrast  = PlayerPrefs.GetInt(K_CONTRAST, 0) == 1;
        bool  dyslexia  = PlayerPrefs.GetInt(K_DYSLEXIA, 0) == 1;
        float gameSpd   = PlayerPrefs.GetFloat(K_GAMESPD, 1f);

        SetToggleSilent(colorblindModeToggle,  cbMode,    OnColorblindModeToggled);
        SetDropdownSilent(colorblindTypeDropdown, cbType,  OnColorblindTypeChanged);
        SetToggleSilent(reducedMotionToggle,   redMotion, OnReducedMotionToggled);
        SetToggleSilent(largeUIToggle,         largeUI,   OnLargeUIToggled);
        SetSliderSilent(uiScaleSlider,         uiScale,   OnUIScaleChanged);
        SetToggleSilent(screenFlashToggle,     flash,     OnScreenFlashToggled);
        SetToggleSilent(highContrastToggle,    contrast,  OnHighContrastToggled);
        SetToggleSilent(dyslexiaFontToggle,    dyslexia,  OnDyslexiaFontToggled);
        SetSliderSilent(gameSpeedSlider,       gameSpd,   OnGameSpeedChanged);

        if (uiScaleLabel)   uiScaleLabel.text   = $"{Mathf.RoundToInt(uiScale * 100)}%";
        if (gameSpeedLabel) gameSpeedLabel.text  = $"{gameSpd:F1}x";
        if (colorblindTypeDropdown)
            colorblindTypeDropdown.interactable = cbMode;

        ApplyAccessibility(cbMode, cbType, redMotion, largeUI, uiScale,
                           flash, contrast, dyslexia, gameSpd);
    }

    // ── APPLY — these actually DO the thing ───────────

    void ApplyAudio(float master, float music, float sfx, bool mute)
    {
        if (audioMixer == null)
        {
            // Fallback if no mixer assigned yet
            AudioListener.volume = mute ? 0f : master;
            return;
        }

        // AudioMixer volumes use logarithmic scale
        // Slider is 0-1, mixer expects dB (-80 to 0)
        audioMixer.SetFloat("MasterVolume", mute ? -80f : LinearToDecibel(master));
        audioMixer.SetFloat("MusicVolume",  LinearToDecibel(music));
        audioMixer.SetFloat("SFXVolume",    LinearToDecibel(sfx));
    }

    void ApplyGraphics(int quality, bool fullscreen, bool vsync, float brightness)
    {
        QualitySettings.SetQualityLevel(quality, true);
        Screen.fullScreen = fullscreen;
        QualitySettings.vSyncCount = vsync ? 1 : 0;

        // Apply brightness via overlay
        StartCoroutine(ApplyBrightnessNextFrame(brightness));
    }

    // Brightness needs one frame delay to ensure overlay is ready
    System.Collections.IEnumerator ApplyBrightnessNextFrame(float brightness)
    {
        yield return null;
        BrightnessOverlay.Instance?.SetBrightness(brightness);
    }

    void ApplyAccessibility(bool cbMode, int cbType, bool reducedMotion,
        bool largeUI, float uiScale, bool flash, bool contrast,
        bool dyslexia, float gameSpeed)
    {
        AccessibilitySettings.ColorblindMode = cbMode;
        AccessibilitySettings.ColorblindType = cbType;
        AccessibilitySettings.ReducedMotion  = reducedMotion;
        AccessibilitySettings.LargeUI        = largeUI;
        AccessibilitySettings.UIScale        = uiScale;
        AccessibilitySettings.ScreenFlash    = flash;
        AccessibilitySettings.HighContrast   = contrast;
        AccessibilitySettings.DyslexiaFont   = dyslexia;
        AccessibilitySettings.GameSpeed      = gameSpeed;

        // UI Scale — actually resize the canvas
        if (mainCanvas != null)
        {
            var scaler = mainCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null)
                scaler.scaleFactor = uiScale;
        }

        // Camera shake off when reduced motion is on
        if (CameraShake.Instance != null)
            CameraShake.Instance.enabled = !reducedMotion;

        // High contrast — swap all backgrounds to black/white
        ApplyHighContrast(contrast);

        // Colorblind mode — apply a color filter overlay
        ApplyColorblindFilter(cbMode, cbType);
    }

    void ApplyHighContrast(bool enabled)
    {
        // Find all Image components in UI and make backgrounds black
        if (!enabled) return;
        var images = FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None);
        foreach (var img in images)
        {
            // Only affect background-style images (semi-transparent ones)
            if (img.color.a < 0.9f && img.color.a > 0.1f)
                img.color = enabled ? new Color(0, 0, 0, 0.95f) : img.color;
        }
    }

    void ApplyColorblindFilter(bool enabled, int type)
    {
        ColorblindOverlay.Instance?.Apply(enabled, type);
    }

    // ── AUDIO CALLBACKS ───────────────────────────────

    public void OnMasterVolumeChanged(float v)
    {
        PlayerPrefs.SetFloat(K_MASTER, v);
        if (masterVolumeLabel)
            masterVolumeLabel.text = $"{Mathf.RoundToInt(v * 100)}%";
        bool mute = muteToggle != null && muteToggle.isOn;
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", mute ? -80f : LinearToDecibel(v));
        else
            AudioListener.volume = mute ? 0f : v;
    }

    public void OnMusicVolumeChanged(float v)
    {
        PlayerPrefs.SetFloat(K_MUSIC, v);
        if (musicVolumeLabel)
            musicVolumeLabel.text = $"{Mathf.RoundToInt(v * 100)}%";
        audioMixer?.SetFloat("MusicVolume", LinearToDecibel(v));
    }

    public void OnSFXVolumeChanged(float v)
    {
        PlayerPrefs.SetFloat(K_SFX, v);
        if (sfxVolumeLabel)
            sfxVolumeLabel.text = $"{Mathf.RoundToInt(v * 100)}%";
        audioMixer?.SetFloat("SFXVolume", LinearToDecibel(v));
    }

    public void OnMuteToggled(bool v)
    {
        PlayerPrefs.SetInt(K_MUTE, v ? 1 : 0);
        float master = masterVolumeSlider != null ? masterVolumeSlider.value : 1f;
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", v ? -80f : LinearToDecibel(master));
        else
            AudioListener.volume = v ? 0f : master;
    }

    // ── GRAPHICS CALLBACKS ────────────────────────────

    public void OnQualityChanged(int v)
    {
        PlayerPrefs.SetInt(K_QUALITY, v);
        QualitySettings.SetQualityLevel(v, true);
    }

    public void OnResolutionChanged(int index)
    {
        var resolutions = Screen.resolutions;
        if (index < 0 || index >= resolutions.Length) return;
        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt(K_RES_INDEX, index);
    }

    public void OnFullscreenToggled(bool v)
    {
        PlayerPrefs.SetInt(K_FULLSCR, v ? 1 : 0);
        Screen.fullScreen = v;
    }

    public void OnVSyncToggled(bool v)
    {
        PlayerPrefs.SetInt(K_VSYNC, v ? 1 : 0);
        QualitySettings.vSyncCount = v ? 1 : 0;
    }

    public void OnBrightnessChanged(float v)
    {
        PlayerPrefs.SetFloat(K_BRIGHT, v);
        if (brightnessLabel)
            brightnessLabel.text = $"{Mathf.RoundToInt(v * 100)}%";
        BrightnessOverlay.Instance?.SetBrightness(v);
    }

    // ── ACCESSIBILITY CALLBACKS ───────────────────────

    public void OnColorblindModeToggled(bool v)
    {
        PlayerPrefs.SetInt(K_CB_MODE, v ? 1 : 0);
        AccessibilitySettings.ColorblindMode = v;
        if (colorblindTypeDropdown) colorblindTypeDropdown.interactable = v;
        ApplyColorblindFilter(v, colorblindTypeDropdown ? colorblindTypeDropdown.value : 0);
    }

    public void OnColorblindTypeChanged(int v)
    {
        PlayerPrefs.SetInt(K_CB_TYPE, v);
        AccessibilitySettings.ColorblindType = v;
        bool cbOn = colorblindModeToggle != null && colorblindModeToggle.isOn;
        ApplyColorblindFilter(cbOn, v);
    }

    public void OnReducedMotionToggled(bool v)
    {
        PlayerPrefs.SetInt(K_REDMOTION, v ? 1 : 0);
        AccessibilitySettings.ReducedMotion = v;
        if (CameraShake.Instance != null) CameraShake.Instance.enabled = !v;
    }

  public void OnLargeUIToggled(bool v)
{
    PlayerPrefs.SetInt(K_LARGE_UI, v ? 1 : 0);
    AccessibilitySettings.LargeUI = v;

    if (mainCanvas != null)
    {
        var scaler = mainCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
        {
            // Large UI = lower reference resolution = bigger elements
            scaler.referenceResolution = v
                ? new Vector2(1280f, 720f)   // bigger UI
                : new Vector2(1920f, 1080f); // normal
        }
    }
}

    public void OnUIScaleChanged(float v)
{
    PlayerPrefs.SetFloat(K_UI_SCALE, v);
    AccessibilitySettings.UIScale = v;
    if (uiScaleLabel) uiScaleLabel.text = $"{Mathf.RoundToInt(v * 100)}%";

    // With Scale With Screen Size, we adjust the reference resolution instead
    // Smaller reference res = bigger UI elements
    if (mainCanvas != null)
    {
        var scaler = mainCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
        {
            // v=1 = normal (1920x1080), v=0.5 = double size UI, v=1.5 = smaller UI
            scaler.referenceResolution = new Vector2(1920f * v, 1080f * v);
        }
    }
}

    public void OnScreenFlashToggled(bool v)
    {
        PlayerPrefs.SetInt(K_FLASH, v ? 1 : 0);
        AccessibilitySettings.ScreenFlash = v;
    }

    public void OnHighContrastToggled(bool v)
    {
        PlayerPrefs.SetInt(K_CONTRAST, v ? 1 : 0);
        AccessibilitySettings.HighContrast = v;
        ApplyHighContrast(v);
    }

    public void OnDyslexiaFontToggled(bool v)
    {
        PlayerPrefs.SetInt(K_DYSLEXIA, v ? 1 : 0);
        AccessibilitySettings.DyslexiaFont = v;
        // Font swap requires a dyslexia-friendly font asset assigned
        // OpenDyslexic is free: https://opendyslexic.org
        DyslexiaFontApplier.Instance?.Apply(v);
    }

    public void OnGameSpeedChanged(float v)
    {
        PlayerPrefs.SetFloat(K_GAMESPD, v);
        AccessibilitySettings.GameSpeed = v;
        if (gameSpeedLabel) gameSpeedLabel.text = $"{v:F1}x";
        // Only apply in gameplay scene, not menu
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
            Time.timeScale = v;
    }

    public void OnResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        LoadAllSettings();
    }

    // ── HELPERS ───────────────────────────────────────

    // Convert 0-1 linear volume to decibels for AudioMixer
    float LinearToDecibel(float linear)
    {
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }

    void SetSliderSilent(Slider s, float v, UnityEngine.Events.UnityAction<float> callback)
    {
        if (s == null) return;
        s.onValueChanged.RemoveListener(callback);
        s.value = v;
        s.onValueChanged.AddListener(callback);
    }

    void SetToggleSilent(Toggle t, bool v, UnityEngine.Events.UnityAction<bool> callback)
    {
        if (t == null) return;
        t.onValueChanged.RemoveListener(callback);
        t.isOn = v;
        t.onValueChanged.AddListener(callback);
    }

    void SetDropdownSilent(TMP_Dropdown d, int v, UnityEngine.Events.UnityAction<int> callback)
    {
        if (d == null) return;
        d.onValueChanged.RemoveListener(callback);
        d.value = v;
        d.onValueChanged.AddListener(callback);
    }

    void UpdateVolumeLabels(float master, float music, float sfx)
    {
        if (masterVolumeLabel) masterVolumeLabel.text = $"{Mathf.RoundToInt(master * 100)}%";
        if (musicVolumeLabel)  musicVolumeLabel.text  = $"{Mathf.RoundToInt(music * 100)}%";
        if (sfxVolumeLabel)    sfxVolumeLabel.text    = $"{Mathf.RoundToInt(sfx * 100)}%";
    }
}













/*using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // ── Audio ─────────────────────────────────────────
    [Header("Audio")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteToggle;
    public TextMeshProUGUI masterVolumeLabel;
    public TextMeshProUGUI musicVolumeLabel;
    public TextMeshProUGUI sfxVolumeLabel;

    // ── Graphics ──────────────────────────────────────
    [Header("Graphics")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public Slider brightnessSlider;
    public TextMeshProUGUI brightnessLabel;

    // ── Accessibility ─────────────────────────────────
    [Header("Accessibility")]
    public Toggle colorblindModeToggle;
    public TMP_Dropdown colorblindTypeDropdown;  // Deuteranopia, Protanopia, Tritanopia
    public Toggle reducedMotionToggle;
    public Toggle largeUIToggle;
    public Slider uiScaleSlider;
    public TextMeshProUGUI uiScaleLabel;
    public Toggle screenFlashToggle;
    public Toggle highContrastToggle;
    public Toggle dyslexiaFontToggle;
    public Slider gameSpeedSlider;
    public TextMeshProUGUI gameSpeedLabel;

    // ── Keys for PlayerPrefs ──────────────────────────
    const string K_MASTER   = "vol_master";
    const string K_MUSIC    = "vol_music";
    const string K_SFX      = "vol_sfx";
    const string K_MUTE     = "mute";
    const string K_QUALITY  = "quality";
    const string K_FULLSCR  = "fullscreen";
    const string K_VSYNC    = "vsync";
    const string K_BRIGHT   = "brightness";
    const string K_CB_MODE  = "cb_mode";
    const string K_CB_TYPE  = "cb_type";
    const string K_REDMOTION= "reduced_motion";
    const string K_LARGE_UI = "large_ui";
    const string K_UI_SCALE = "ui_scale";
    const string K_FLASH    = "screen_flash";
    const string K_CONTRAST = "high_contrast";
    const string K_DYSLEXIA = "dyslexia_font";
    const string K_GAMESPD  = "game_speed";

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadAllSettings();
        PopulateResolutionDropdown();
        PopulateQualityDropdown();
    }

    // ── LOAD ──────────────────────────────────────────

    void LoadAllSettings()
    {
        // Audio
        float master = PlayerPrefs.GetFloat(K_MASTER, 1f);
        float music  = PlayerPrefs.GetFloat(K_MUSIC,  0.8f);
        float sfx    = PlayerPrefs.GetFloat(K_SFX,    1f);
        bool  mute   = PlayerPrefs.GetInt(K_MUTE, 0) == 1;

        SetSlider(masterVolumeSlider, master);
        SetSlider(musicVolumeSlider, music);
        SetSlider(sfxVolumeSlider, sfx);
        SetToggle(muteToggle, mute);
        UpdateLabels();
        ApplyAudio(master, music, sfx, mute);

        // Graphics
        int quality    = PlayerPrefs.GetInt(K_QUALITY, QualitySettings.GetQualityLevel());
        bool fullscreen = PlayerPrefs.GetInt(K_FULLSCR, Screen.fullScreen ? 1 : 0) == 1;
        bool vsync     = PlayerPrefs.GetInt(K_VSYNC, 1) == 1;
        float bright   = PlayerPrefs.GetFloat(K_BRIGHT, 1f);

        if (qualityDropdown)    qualityDropdown.value = quality;
        SetToggle(fullscreenToggle, fullscreen);
        SetToggle(vsyncToggle, vsync);
        SetSlider(brightnessSlider, bright);
        ApplyGraphics(quality, fullscreen, vsync, bright);

        // Accessibility
        bool cbMode   = PlayerPrefs.GetInt(K_CB_MODE, 0) == 1;
        int  cbType   = PlayerPrefs.GetInt(K_CB_TYPE, 0);
        bool redMotion = PlayerPrefs.GetInt(K_REDMOTION, 0) == 1;
        bool largeUI  = PlayerPrefs.GetInt(K_LARGE_UI, 0) == 1;
        float uiScale = PlayerPrefs.GetFloat(K_UI_SCALE, 1f);
        bool flash    = PlayerPrefs.GetInt(K_FLASH, 1) == 1;
        bool contrast = PlayerPrefs.GetInt(K_CONTRAST, 0) == 1;
        bool dyslexia = PlayerPrefs.GetInt(K_DYSLEXIA, 0) == 1;
        float gameSpd = PlayerPrefs.GetFloat(K_GAMESPD, 1f);

        SetToggle(colorblindModeToggle, cbMode);
        if (colorblindTypeDropdown) colorblindTypeDropdown.value = cbType;
        SetToggle(reducedMotionToggle, redMotion);
        SetToggle(largeUIToggle, largeUI);
        SetSlider(uiScaleSlider, uiScale);
        SetToggle(screenFlashToggle, flash);
        SetToggle(highContrastToggle, contrast);
        SetToggle(dyslexiaFontToggle, dyslexia);
        SetSlider(gameSpeedSlider, gameSpd);

        ApplyAccessibility(cbMode, cbType, redMotion, largeUI, uiScale, flash, contrast, dyslexia, gameSpd);
    }

    // ── APPLY ─────────────────────────────────────────

    void ApplyAudio(float master, float music, float sfx, bool mute)
    {
        float effective = mute ? 0f : master;
        AudioListener.volume = effective;
        // If you have an AudioMixer, set mixer groups here instead
    }

    void ApplyGraphics(int quality, bool fullscreen, bool vsync, float brightness)
    {
        QualitySettings.SetQualityLevel(quality);
        Screen.fullScreen = fullscreen;
        QualitySettings.vSyncCount = vsync ? 1 : 0;
        // Brightness: adjust via post-processing or a full-screen overlay alpha
        if (BrightnessOverlay.Instance != null)
            BrightnessOverlay.Instance.SetBrightness(brightness);
    }

    void ApplyAccessibility(bool cbMode, int cbType, bool reducedMotion,
        bool largeUI, float uiScale, bool flash, bool contrast,
        bool dyslexia, float gameSpeed)
    {
        // These values are read by other systems at runtime
        // Store them as static values for easy access
        AccessibilitySettings.ColorblindMode    = cbMode;
        AccessibilitySettings.ColorblindType    = cbType;
        AccessibilitySettings.ReducedMotion     = reducedMotion;
        AccessibilitySettings.LargeUI           = largeUI;
        AccessibilitySettings.UIScale           = uiScale;
        AccessibilitySettings.ScreenFlash       = flash;
        AccessibilitySettings.HighContrast      = contrast;
        AccessibilitySettings.DyslexiaFont      = dyslexia;
        AccessibilitySettings.GameSpeed         = gameSpeed;

        // Apply camera shake disable
        if (reducedMotion && CameraShake.Instance != null)
            CameraShake.Instance.enabled = false;
        else if (CameraShake.Instance != null)
            CameraShake.Instance.enabled = true;
    }

    // ── UI CALLBACKS (wire these to UI elements OnValueChanged) ──

    public void OnMasterVolumeChanged(float v)
    {
        PlayerPrefs.SetFloat(K_MASTER, v);
        if (masterVolumeLabel) masterVolumeLabel.text = $"{Mathf.RoundToInt(v * 100)}%";
        ApplyAudio(v, GetSliderValue(musicVolumeSlider),
                      GetSliderValue(sfxVolumeSlider),
                      GetToggleValue(muteToggle));
    }

    public void OnMusicVolumeChanged(float v)
    {
        PlayerPrefs.SetFloat(K_MUSIC, v);
        if (musicVolumeLabel) musicVolumeLabel.text = $"{Mathf.RoundToInt(v * 100)}%";
    }

    public void OnSFXVolumeChanged(float v)
    {
        PlayerPrefs.SetFloat(K_SFX, v);
        if (sfxVolumeLabel) sfxVolumeLabel.text = $"{Mathf.RoundToInt(v * 100)}%";
    }

    public void OnMuteToggled(bool v)
    {
        PlayerPrefs.SetInt(K_MUTE, v ? 1 : 0);
        ApplyAudio(GetSliderValue(masterVolumeSlider),
                   GetSliderValue(musicVolumeSlider),
                   GetSliderValue(sfxVolumeSlider), v);
    }

    public void OnResolutionChanged(int index)
{
    var resolutions = Screen.resolutions;
    if (index < 0 || index >= resolutions.Length) return;
    Resolution r = resolutions[index];
    Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    PlayerPrefs.SetInt("resolution_index", index);
}

    public void OnQualityChanged(int v)
    {
        PlayerPrefs.SetInt(K_QUALITY, v);
        QualitySettings.SetQualityLevel(v);
    }

    public void OnFullscreenToggled(bool v)
    {
        PlayerPrefs.SetInt(K_FULLSCR, v ? 1 : 0);
        Screen.fullScreen = v;
    }

    public void OnVSyncToggled(bool v)
    {
        PlayerPrefs.SetInt(K_VSYNC, v ? 1 : 0);
        QualitySettings.vSyncCount = v ? 1 : 0;
    }

    public void OnBrightnessChanged(float v)
    {
        PlayerPrefs.SetFloat(K_BRIGHT, v);
        if (brightnessLabel) brightnessLabel.text = $"{Mathf.RoundToInt(v * 100)}%";
        BrightnessOverlay.Instance?.SetBrightness(v);
    }

    public void OnColorblindModeToggled(bool v)
    {
        PlayerPrefs.SetInt(K_CB_MODE, v ? 1 : 0);
        AccessibilitySettings.ColorblindMode = v;
        if (colorblindTypeDropdown) colorblindTypeDropdown.interactable = v;
    }

    public void OnColorblindTypeChanged(int v)
    {
        PlayerPrefs.SetInt(K_CB_TYPE, v);
        AccessibilitySettings.ColorblindType = v;
    }

    public void OnReducedMotionToggled(bool v)
    {
        PlayerPrefs.SetInt(K_REDMOTION, v ? 1 : 0);
        AccessibilitySettings.ReducedMotion = v;
        if (CameraShake.Instance != null) CameraShake.Instance.enabled = !v;
    }

    public void OnLargeUIToggled(bool v)
    {
        PlayerPrefs.SetInt(K_LARGE_UI, v ? 1 : 0);
        AccessibilitySettings.LargeUI = v;
    }

    public void OnUIScaleChanged(float v)
    {
        PlayerPrefs.SetFloat(K_UI_SCALE, v);
        AccessibilitySettings.UIScale = v;
        if (uiScaleLabel) uiScaleLabel.text = $"{Mathf.RoundToInt(v * 100)}%";
    }

    public void OnScreenFlashToggled(bool v)
    {
        PlayerPrefs.SetInt(K_FLASH, v ? 1 : 0);
        AccessibilitySettings.ScreenFlash = v;
    }

    public void OnHighContrastToggled(bool v)
    {
        PlayerPrefs.SetInt(K_CONTRAST, v ? 1 : 0);
        AccessibilitySettings.HighContrast = v;
    }

    public void OnDyslexiaFontToggled(bool v)
    {
        PlayerPrefs.SetInt(K_DYSLEXIA, v ? 1 : 0);
        AccessibilitySettings.DyslexiaFont = v;
    }

    public void OnGameSpeedChanged(float v)
    {
        PlayerPrefs.SetFloat(K_GAMESPD, v);
        AccessibilitySettings.GameSpeed = v;
        if (gameSpeedLabel) gameSpeedLabel.text = $"{v:F1}x";
    }

    public void OnResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        LoadAllSettings();
    }

    // ── Helpers ───────────────────────────────────────

    void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        int current = 0;
        var resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].width} x {resolutions[i].height}");
            if (resolutions[i].width  == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                current = i;
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = current;
    }

    void PopulateQualityDropdown()
    {
        if (qualityDropdown == null) return;
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(
            new System.Collections.Generic.List<string>(QualitySettings.names));
    }

    void UpdateLabels()
    {
        if (masterVolumeLabel)
            masterVolumeLabel.text = $"{Mathf.RoundToInt(GetSliderValue(masterVolumeSlider) * 100)}%";
        if (musicVolumeLabel)
            musicVolumeLabel.text  = $"{Mathf.RoundToInt(GetSliderValue(musicVolumeSlider) * 100)}%";
        if (sfxVolumeLabel)
            sfxVolumeLabel.text    = $"{Mathf.RoundToInt(GetSliderValue(sfxVolumeSlider) * 100)}%";
    }

    void SetSlider(Slider s, float v)   { if (s) s.value = v; }
    void SetToggle(Toggle t, bool v)    { if (t) t.isOn  = v; }
    float GetSliderValue(Slider s)      => s ? s.value : 0f;
    bool  GetToggleValue(Toggle t)      => t != null && t.isOn;
}
*/