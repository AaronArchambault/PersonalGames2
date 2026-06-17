using UnityEngine;
using System.Collections;

/// <summary>
/// Obstacle — damages and shrinks the player on contact.
/// Examples: jellyfish, mines, hooks, whirlpools, sea urchins.
/// Attach to a GameObject with a Collider2D set to Is Trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Obstacle : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("How much mass is removed from the player on hit.")]
    public float massDamage       = 8f;
    [Tooltip("Seconds the player is invincible after being hit (prevent multi-hit).")]
    public float iframeDuration   = 1.2f;

    [Header("Knockback")]
    public float knockbackForce   = 6f;

    [Header("Movement (optional)")]
    [Tooltip("Does this obstacle drift/move?")]
    public bool  moves            = false;
    public float driftSpeed       = 0.5f;
    [Tooltip("Direction the obstacle drifts (local space, normalized automatically).")]
    public Vector2 driftDirection = Vector2.left;
    [Tooltip("If true, the obstacle bounces between world-space bounds.")]
    public bool  bounces          = false;
    public float bounceMinX       = -20f;
    public float bounceMaxX       =  20f;

    [Header("Visual Feedback")]
    public GameObject hitVFXPrefab;
    public Color      flashColor  = new Color(1f, 0.3f, 0.3f, 1f);

    // ── Runtime ──────────────────────────────────────────
    private Rigidbody2D     obstacleRb;
    private SpriteRenderer  playerRenderer;

    void Awake()
    {
        // Ensure trigger
        foreach (var c in GetComponents<Collider2D>())
            c.isTrigger = true;

        if (moves)
        {
            obstacleRb = GetComponent<Rigidbody2D>();
            if (obstacleRb == null) obstacleRb = gameObject.AddComponent<Rigidbody2D>();
            obstacleRb.gravityScale  = 0f;
            obstacleRb.linearDamping = 0f;
        }

        driftDirection = driftDirection.normalized;
    }

    void FixedUpdate()
    {
        if (!moves || obstacleRb == null) return;

        obstacleRb.MovePosition(obstacleRb.position + driftDirection * driftSpeed * Time.fixedDeltaTime);

        if (bounces)
        {
            float x = obstacleRb.position.x;
            if (x < bounceMinX || x > bounceMaxX)
                driftDirection.x *= -1f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerCreature player = other.GetComponent<PlayerCreature>();
        if (player == null) return;
        if (player.IsInvincible) return;

        // Deal damage
        player.TakeDamage(massDamage, knockbackForce, transform.position, iframeDuration);

        // Hit VFX
        if (hitVFXPrefab != null)
            Instantiate(hitVFXPrefab, other.transform.position, Quaternion.identity);
    }
}