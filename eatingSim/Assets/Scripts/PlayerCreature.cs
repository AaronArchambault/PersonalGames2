using UnityEngine;
using System.Collections;

/// <summary>
/// PlayerCreature — movement, eating, growing, and taking damage.
/// Attach to the Player GameObject. Tag the GameObject "Player".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerCreature : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Movement")]
    public float baseSpeed       = 5f;
    public float smoothing       = 0.15f;

    [Header("Growth")]
    public float startSize       = 0.5f;
    public float massPerSizeDouble = 50f;
    public float maxSize         = 20f;

    [Header("Damage")]
    [Tooltip("Minimum size the player can shrink to before game over.")]
    public float minSize         = 0.15f;
    public GameObject hitVFXPrefab;
    public AudioClip  hurtSound;

    [Header("Audio")]
    public AudioClip eatSoundSmall;
    public AudioClip eatSoundLarge;

    [Header("VFX")]
    public GameObject eatParticlePrefab;

    // ── Public state ──────────────────────────────────────
    public float Mass         { get; private set; } = 0f;
    public float CurrentSize  => Mathf.Abs(transform.localScale.x);
    public bool  IsInvincible { get; private set; } = false;

    // ── Private ───────────────────────────────────────────
    private Rigidbody2D    rb;
    private CircleCollider2D col;
    private AudioSource    audioSource;
    private SpriteRenderer sr;
    private Vector2        targetPosition;
    private Camera         mainCam;

    // ── Unity Lifecycle ───────────────────────────────────
    void Awake()
    {
        rb          = GetComponent<Rigidbody2D>();
        col         = GetComponent<CircleCollider2D>();
        sr          = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        mainCam        = Camera.main;
        rb.gravityScale = 0f;
        rb.drag         = 2f;

        transform.localScale = Vector3.one * startSize;
        targetPosition       = rb.position;
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        MoveTowardsTarget();
    }

    // ── Input ─────────────────────────────────────────────
    void HandleInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 wp     = mainCam.ScreenToWorldPoint(Input.mousePosition);
            targetPosition = new Vector2(wp.x, wp.y);
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            Vector2 dir    = new Vector2(h, v).normalized;
            targetPosition = rb.position + dir * 2f;
        }
    }

    void MoveTowardsTarget()
    {
        float speedScale = 1f / Mathf.Max(1f, CurrentSize * 0.4f);
        Vector2 next     = Vector2.Lerp(rb.position, targetPosition, smoothing);
        rb.MovePosition(next);

        // Flip sprite
        float dx = targetPosition.x - rb.position.x;
        float sz = CurrentSize;
        if (dx >  0.05f) transform.localScale = new Vector3( sz, sz, 1f);
        if (dx < -0.05f) transform.localScale = new Vector3(-sz, sz, 1f);
    }

    // ── Eating ────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        Edible edible = other.GetComponent<Edible>();
        if (edible == null) return;

        if (CanEat(edible))
            EatObject(edible);
        else
        {
            // Bounce away — too big to eat
            Vector2 push = (rb.position - (Vector2)other.transform.position).normalized;
            rb.AddForce(push * 8f, ForceMode2D.Impulse);
        }
    }

    void EatObject(Edible edible)
    {
        Mass += edible.massValue;
        ApplyGrowth();

        PlayEatSound(edible.massValue);
        SpawnParticle(edible.transform.position, eatParticlePrefab);
        GameManager.Instance?.OnPlayerAte(edible.massValue, Mass);
        edible.OnEaten();
    }

    void ApplyGrowth()
    {
        float newSize = startSize + Mathf.Log(1f + Mass / massPerSizeDouble, 2f) * startSize;
        newSize       = Mathf.Clamp(newSize, startSize, maxSize);
        SetSize(newSize);
    }

    public bool CanEat(Edible edible) =>
        edible.massValue <= Mass + startSize * massPerSizeDouble;

    // ── Taking Damage ─────────────────────────────────────
    /// <summary>
    /// Called by Obstacle and Enemy when they hit the player.
    /// </summary>
    public void TakeDamage(float massDamage, float knockbackForce, Vector3 sourcePosition, float iframeDuration)
    {
        if (IsInvincible) return;

        Mass = Mathf.Max(0f, Mass - massDamage);

        // Shrink
        float newSize = startSize + Mathf.Log(1f + Mass / massPerSizeDouble, 2f) * startSize;
        newSize       = Mathf.Max(newSize, minSize);
        SetSize(newSize);

        // Knockback
        Vector2 push = (rb.position - (Vector2)sourcePosition).normalized;
        rb.AddForce(push * knockbackForce, ForceMode2D.Impulse);

        // VFX / SFX
        SpawnParticle(transform.position, hitVFXPrefab);
        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        // Notify UI/manager
        GameManager.Instance?.OnPlayerDamaged(massDamage, Mass);

        // Check death
        if (CurrentSize <= minSize + 0.01f)
        {
            GameManager.Instance?.TriggerGameOver();
            return;
        }

        StartCoroutine(IFrameRoutine(iframeDuration));
    }

    IEnumerator IFrameRoutine(float duration)
    {
        IsInvincible = true;

        // Flash the sprite during iframes
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        if (sr != null) sr.enabled = true;
        IsInvincible = false;
    }

    // ── Helpers ───────────────────────────────────────────
    void SetSize(float size)
    {
        float signX   = Mathf.Sign(transform.localScale.x); // preserve flip
        transform.localScale = new Vector3(size * signX, size, 1f);
    }

    void PlayEatSound(float mass)
    {
        AudioClip clip = mass < 5f ? eatSoundSmall : eatSoundLarge;
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    void SpawnParticle(Vector3 pos, GameObject prefab)
    {
        if (prefab != null) Instantiate(prefab, pos, Quaternion.identity);
    }
}