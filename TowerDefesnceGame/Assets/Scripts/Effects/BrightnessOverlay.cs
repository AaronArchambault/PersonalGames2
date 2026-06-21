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
        if (overlayImage) overlayImage.raycastTarget = false;
    }
 
    public void SetBrightness(float brightness)
    {
        if (overlayImage == null) return;
        if (brightness < 1f)
            overlayImage.color = new Color(0, 0, 0, 1f - brightness);
        else
            overlayImage.color = new Color(1, 1, 1, brightness - 1f);
    }
 
    // NEW — set a specific tinted color for screen flashes
    public void SetColor(Color color)
    {
        if (overlayImage == null) return;
        overlayImage.color = color;
    }
 
    public void SetColorAlpha(float alpha)
    {
        if (overlayImage == null) return;
        Color c = overlayImage.color;
        c.a = alpha;
        overlayImage.color = c;
    }
}