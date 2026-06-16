using UnityEngine;

/// <summary>
/// Edible — attach to any object the player can eat.
/// Handles its own wander/flee AI and death VFX.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Edible : MonoBehaviour
{
    [Header("Identity")]
    public string displayName  = "Fish";
    public float  massValue    = 1f;

    [Header("Movement")]
    public bool  selfPropelled   = true;
    public float moveSpeed       = 1.5f;
    public bool  fleesFromPlayer = true;
    public float fleeRadius      = 3f;

    [Header("Visual")]
    public GameObject deathVFXPrefab;

    // ── Runtime ──────────────────────────────────────────
    private Rigidbody2D rb;
    private Transform   playerTransform;
    private Vector2     wanderTarget;
    private float       wanderTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.drag         = 1f;

        foreach (var c in GetComponents<Collider2D>())
            c.isTrigger = true;

        PickNewWanderTarget();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) playerTransform = p.transform;
    }

    void FixedUpdate()
    {
        if (!selfPropelled) return;

        if (fleesFromPlayer && playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist < fleeRadius) { Flee(); return; }
        }
        Wander();
    }

    void Wander()
    {
        wanderTimer -= Time.fixedDeltaTime;
        if (wanderTimer <= 0f) PickNewWanderTarget();

        Vector2 dir = (wanderTarget - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        FlipSprite(dir.x);
    }

    void Flee()
    {
        Vector2 away = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
        rb.MovePosition(rb.position + away * moveSpeed * 1.8f * Time.fixedDeltaTime);
        FlipSprite(away.x);
    }

    void PickNewWanderTarget()
    {
        wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * 5f;
        wanderTimer  = Random.Range(2f, 5f);
    }

    void FlipSprite(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f) return;
        Vector3 s = transform.localScale;
        s.x = (dirX > 0) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    public void OnEaten()
    {
        if (deathVFXPrefab != null)
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}