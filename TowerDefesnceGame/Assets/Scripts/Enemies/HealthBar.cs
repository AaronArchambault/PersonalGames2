using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public SpriteRenderer fillSprite;
    public SpriteRenderer bgSprite;

    private float fullScaleX;

    void Awake()
    {
        if (fillSprite) fullScaleX = fillSprite.transform.localScale.x;
    }

    public void SetFill(float normalised)
    {
        if (fillSprite == null) return;
        Vector3 s = fillSprite.transform.localScale;
        s.x = fullScaleX * Mathf.Clamp01(normalised);
        fillSprite.transform.localScale = s;

        // Color shifts from green → yellow → red
        fillSprite.color = Color.Lerp(Color.red, Color.green, normalised);
    }
}