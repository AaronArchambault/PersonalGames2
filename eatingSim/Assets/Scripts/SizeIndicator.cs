using UnityEngine;

/// <summary>
/// SizeIndicator — colours the Edible green (eatable) or red (avoid).
/// Attach to every Edible prefab alongside SpriteRenderer.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SizeIndicator : MonoBehaviour
{
    public Color edibleColor = new Color(0.6f, 1f, 0.6f, 1f);
    public Color dangerColor = new Color(1f, 0.4f, 0.4f, 1f);
    public Color neutralColor = Color.white;
    public float refreshInterval = 0.25f;

    private SpriteRenderer sr;
    private PlayerCreature player;
    private Edible         edible;
    private float          timer;

    void Awake()
    {
        sr     = GetComponent<SpriteRenderer>();
        edible = GetComponent<Edible>();
    }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p) player = p.GetComponent<PlayerCreature>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = refreshInterval;

        if (player == null || edible == null) { sr.color = neutralColor; return; }
        sr.color = player.CanEat(edible) ? edibleColor : dangerColor;
    }
}