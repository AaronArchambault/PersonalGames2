// Scripts/Effects/ColorblindOverlay.cs — full replacement
using UnityEngine;
using UnityEngine.UI;

public class ColorblindOverlay : MonoBehaviour
{
    public static ColorblindOverlay Instance { get; private set; }

    private Image overlayImage;

    // Subtle tint colors per colorblind type
    static readonly Color[] tints =
    {
        new Color(1f, 0.85f, 0f, 0.12f),  // Deuteranopia
        new Color(1f, 0.6f,  0f, 0.12f),  // Protanopia
        new Color(0f, 0.75f, 1f, 0.12f),  // Tritanopia
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        overlayImage = GetComponent<Image>();
        if (overlayImage != null)
        {
            overlayImage.raycastTarget = false;
            overlayImage.color = new Color(0, 0, 0, 0); // Start invisible
        }
    }

    public void Apply(bool enabled, int type)
    {
        if (overlayImage == null) return;
        if (!enabled)
        {
            overlayImage.color = new Color(0, 0, 0, 0);
            return;
        }
        if (type >= 0 && type < tints.Length)
            overlayImage.color = tints[type];
    }
}