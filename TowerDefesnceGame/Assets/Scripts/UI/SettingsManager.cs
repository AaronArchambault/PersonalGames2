using UnityEngine;
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