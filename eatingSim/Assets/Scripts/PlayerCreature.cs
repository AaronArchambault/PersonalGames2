using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// PlayerCreature — movement, eating, growing, and taking damage.
/// Uses the NEW Unity Input System (Mouse + Keyboard classes).
/// Attach to the Player GameObject and tag it "Player".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerCreature : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Movement")]
    public float baseSpeed        = 5f;
    public float smoothing        = 0.15f;

    [Header("Growth")]
    public float startSize        = 0.5f;
    public float massPerSizeDouble = 50f;
    public float maxSize          = 20f;

    [Header("Damage")]
    public float minSize          = 0.15f;
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
    private Rigidbody2D      rb;
    private CircleCollider2D col;
    private AudioSource      audioSource;
    private SpriteRenderer   sr;
    private Vector2          targetPosition;
    private Camera           mainCam;

    // New Input System device references
    private Mouse    mouse;
    private Keyboard keyboard;

    // ── Unity Lifecycle ───────────────────────────────────
    void Awake()
    {
        rb          = GetComponent<Rigidbody2D>();
        col         = GetComponent<CircleCollider2D>();
        sr          = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        mainCam          = Camera.main;
        rb.gravityScale  = 0f;
        rb.linearDamping = 2f;

        transform.localScale = Vector3.one * startSize;
        targetPosition       = rb.position;

        mouse    = Mouse.current;
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (mouse    == null) mouse    = Mouse.current;
        if (keyboard == null) keyboard = Keyboard.current;
        HandleInput();
    }

    void FixedUpdate()
    {
        MoveTowardsTarget();
    }

    // ── Input (New Input System) ──────────────────────────
    void HandleInput()
    {
        // Mouse: follow cursor while left button held
        if (mouse != null && mouse.leftButton.isPressed)
        {
            Vector2 screenPos = mouse.position.ReadValue();
            Vector3 wp        = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            targetPosition    = new Vector2(wp.x, wp.y);
        }

        // Keyboard: WASD / Arrow keys
        if (keyboard != null)
        {
            float h = 0f, v = 0f;
            if (keyboard.aKey.isPressed    || keyboard.leftArrowKey.isPressed)  h -= 1f;
            if (keyboard.dKey.isPressed    || keyboard.rightArrowKey.isPressed) h += 1f;
            if (keyboard.sKey.isPressed    || keyboard.downArrowKey.isPressed)  v -= 1f;
            if (keyboard.wKey.isPressed    || keyboard.upArrowKey.isPressed)    v += 1f;

            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                Vector2 dir    = new Vector2(h, v).normalized;
                targetPosition = rb.position + dir * 2f;
            }
        }
    }

    void MoveTowardsTarget()
    {
        Vector2 next = Vector2.Lerp(rb.position, targetPosition, smoothing);
        rb.MovePosition(next);

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
    public void TakeDamage(float massDamage, float knockbackForce, Vector3 sourcePosition, float iframeDuration)
    {
        if (IsInvincible) return;

        Mass = Mathf.Max(0f, Mass - massDamage);

        float newSize = startSize + Mathf.Log(1f + Mass / massPerSizeDouble, 2f) * startSize;
        newSize       = Mathf.Max(newSize, minSize);
        SetSize(newSize);

        Vector2 push = (rb.position - (Vector2)sourcePosition).normalized;
        rb.AddForce(push * knockbackForce, ForceMode2D.Impulse);

        SpawnParticle(transform.position, hitVFXPrefab);
        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        GameManager.Instance?.OnPlayerDamaged(massDamage, Mass);

        if (CurrentSize <= minSize + 0.01f)
        {
            GameManager.Instance?.TriggerGameOver();
            return;
        }

        StartCoroutine(IFrameRoutine(iframeDuration));
    }

    System.Collections.IEnumerator IFrameRoutine(float duration)
    {
        IsInvincible = true;
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
        float signX          = Mathf.Sign(transform.localScale.x);
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