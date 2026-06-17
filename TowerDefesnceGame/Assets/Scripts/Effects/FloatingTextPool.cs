// FloatingTextPool.cs
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance { get; private set; }
    public string floatingTextTag = "FloatingText";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Spawn(Vector3 pos, string text, Color color)
    {
        var obj = ObjectPool.Instance.Spawn(floatingTextTag, pos, Quaternion.identity);
        if (obj == null) return;
        obj.GetComponent<FloatingText>()?.Setup(text, color);
    }
} 