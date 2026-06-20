using UnityEngine;
using UnityEngine.UI;

public class BrightnessOverlay : MonoBehaviour
{
    public static BrightnessOverlay Instance { get; private set; }

    private Image overlayImage;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        overlayImage = GetComponent<Image>();
    }

    // brightness 0 = black screen, 1 = normal, values above 1 lighten
    public void SetBrightness(float brightness)
    {
        if (overlayImage == null) return;
        if (brightness < 1f)
        {
            // Darken: black overlay with increasing opacity
            overlayImage.color = new Color(0, 0, 0, 1f - brightness);
        }
        else
        {
            // Lighten: white overlay
            overlayImage.color = new Color(1, 1, 1, brightness - 1f);
        }
    }
}