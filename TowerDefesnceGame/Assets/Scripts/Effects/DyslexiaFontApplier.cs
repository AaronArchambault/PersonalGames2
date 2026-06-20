using UnityEngine;
using TMPro;

public class DyslexiaFontApplier : MonoBehaviour
{
    public static DyslexiaFontApplier Instance { get; private set; }

    [Tooltip("Download OpenDyslexic free from opendyslexic.org, import as TMP font asset")]
    public TMP_FontAsset dyslexiaFont;
    public TMP_FontAsset normalFont;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Apply(bool useDyslexia)
    {
        if (dyslexiaFont == null || normalFont == null)
        {
            Debug.LogWarning("DyslexiaFontApplier: font assets not assigned.");
            return;
        }
        TMP_FontAsset target = useDyslexia ? dyslexiaFont : normalFont;
        var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var t in allTexts)
            t.font = target;
    }
}