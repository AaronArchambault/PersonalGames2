using UnityEngine;

/// <summary>
/// Enemy — a creature bigger than the player that swims around and
/// chases/attacks when the player gets close. If the player grows bigger
/// than the enemy, it turns into an Edible instead (optional).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    public enum EnemyState { Wander, Chase, Flee }

    [Header("Stats")]
    [Tooltip("How much mass this enemy represents (used for eat-check).")]
    public float massThreshold  = 30f;
    [Tooltip("Damage dealt to the player per hit.")]
    public float massDamage     = 12f;
    public float knockbackForce = 8f;
    public float iframeDuration = 1.5f;

    [Header("Movement")]
    public float wanderSpeed    = 1.2f;
    public float chaseSpeed     = 2.8f;
    public float fleeSpeed      = 3.5f;
    [Tooltip("Distance at which the enemy starts chasing the player.")]
    public float detectRadius   = 5f;
    [Tooltip("Distance at which the enemy gives up chasing.")]
    public float loseRadius     = 9f;

    [Header("Edible Conversion")]
    [Tooltip("When the player outgrows this enemy, it becomes edible.")]
    public bool  convertToEdibleWhenOutgrown = true;
    public float edibleMassValue             = 20f;
    public GameObject deathVFXPrefab;

    [Header("Visual")]
    [Tooltip("Tint when in chase mode.")]
    public Color chaseColor  = new Color(1f, 0.5f, 0.5f);
    [Tooltip("Tint when in wander mode.")]
    public Color wanderColor = Color.white;

    // ── Runtime ──────────────────────────────────────────
    private Rigidbody2D    rb;
    private SpriteRenderer sr;
    private PlayerCreature player;
    private EnemyState     state          = EnemyState.Wander;
    private Vector2        wanderTarget;
    private float          wanderTimer;
    private bool           convertedToEdible = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.drag = 1.5f;

        sr = GetComponent<SpriteRenderer>();

        foreach (var c in GetComponents<Collider2D>())
            c.isTrigger = true;

        PickNewWanderTarget();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.GetComponent<PlayerCreature>();
    }

    void Update()
    {
        if (convertedToEdible) return;
        if (player == null) return;

        // Check if player has outgrown this enemy
        if (convertToEdibleWhenOutgrown && player.Mass >= massThreshold)
        {
            ConvertToEdible();
            return;
        }

        UpdateState();
        UpdateColor();
    }

    void FixedUpdate()
    {
        if (convertedToEdible || player == null) return;

        switch (state)
        {
            case EnemyState.Wander: DoWander();  break;
            case EnemyState.Chase:  DoChase();   break;
            case EnemyState.Flee:   DoFlee();    break;
        }
    }

    // ── State Machine ─────────────────────────────────────
    void UpdateState()
    {
        float dist = Vector2.Distance(transform.position, player.transform.position);

        if (state == EnemyState.Wander && dist < detectRadius)
            state = EnemyState.Chase;
        else if (state == EnemyState.Chase && dist > loseRadius)
            state = EnemyState.Wander;
    }

    // ── Movement ──────────────────────────────────────────
    void DoWander()
    {
        wanderTimer -= Time.fixedDeltaTime;
        if (wanderTimer <= 0f) PickNewWanderTarget();

        Vector2 dir = (wanderTarget - rb.position).normalized;
        rb.MovePosition(rb.position + dir * wanderSpeed * Time.fixedDeltaTime);
        FlipSprite(dir.x);
    }

    void DoChase()
    {
        Vector2 dir = ((Vector2)player.transform.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * chaseSpeed * Time.fixedDeltaTime);
        FlipSprite(dir.x);
    }

    void DoFlee()
    {
        Vector2 dir = (rb.position - (Vector2)player.transform.position).normalized;
        rb.MovePosition(rb.position + dir * fleeSpeed * Time.fixedDeltaTime);
        FlipSprite(dir.x);
    }

    void PickNewWanderTarget()
    {
        wanderTarget = rb.position + Random.insideUnitCircle * 6f;
        wanderTimer  = Random.Range(2f, 5f);
    }

    void FlipSprite(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f || sr == null) return;
        Vector3 s = transform.localScale;
        s.x = dirX > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void UpdateColor()
    {
        if (sr == null) return;
        sr.color = (state == EnemyState.Chase) ? chaseColor : wanderColor;
    }

    // ── Collision ─────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (convertedToEdible) return;

        PlayerCreature p = other.GetComponent<PlayerCreature>();
        if (p == null || p.IsInvincible) return;

        p.TakeDamage(massDamage, knockbackForce, transform.position, iframeDuration);
    }

    // ── Conversion ────────────────────────────────────────
    void ConvertToEdible()
    {
        convertedToEdible = true;

        // Swap Enemy component for Edible so the player can eat it
        var edible = gameObject.AddComponent<Edible>();
        edible.massValue        = edibleMassValue;
        edible.displayName      = "Defeated Enemy";
        edible.fleesFromPlayer  = true;
        edible.moveSpeed        = 2f;
        edible.deathVFXPrefab   = deathVFXPrefab;

        // Change colour to green to signal it's now eatable
        if (sr != null) sr.color = new Color(0.6f, 1f, 0.6f);

        // Disable this component (Edible takes over)
        this.enabled = false;
    }
}