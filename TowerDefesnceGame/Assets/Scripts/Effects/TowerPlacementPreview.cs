using UnityEngine;

public class TowerPlacementPreview : MonoBehaviour
{
    private SpriteRenderer sr;
    public Color validColor   = new Color(0f, 1f, 0f, 0.5f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void SetValid(bool valid) =>
        sr.color = valid ? validColor : invalidColor;
}