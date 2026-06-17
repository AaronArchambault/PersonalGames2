// FloatingText.cs
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour, IPoolable
{
    public string poolTag = "FloatingText";
    public TextMeshPro tmp;

    private float lifetime = 1f;
    private float elapsed;
    private Vector3 startPos;
    private Color   startColor;

    public void OnCreated(string tag) => poolTag = tag;
    public void OnDespawn() => elapsed = 0f;
    public void OnSpawn()   => elapsed = 0f;

    public void Setup(string text, Color color)
    {
        tmp.text  = text;
        tmp.color = color;
        startPos  = transform.position;
        startColor = color;
        elapsed   = 0f;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / lifetime;

        // Float upward
        transform.position = startPos + Vector3.up * t * 0.8f;

        // Fade out
        Color c = startColor;
        c.a = Mathf.Lerp(1f, 0f, t);
        tmp.color = c;

        // Scale pop
        float scale = t < 0.1f ? Mathf.Lerp(0f, 1.2f, t / 0.1f)
                    : t < 0.2f ? Mathf.Lerp(1.2f, 1f, (t - 0.1f) / 0.1f)
                    : 1f;
        transform.localScale = Vector3.one * scale;

        if (elapsed >= lifetime)
            ObjectPool.Instance.Despawn(poolTag, gameObject);
    }
}