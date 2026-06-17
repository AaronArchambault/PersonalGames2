using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour, IPoolable
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public int reward = 10;
    public int liveDamage = 1;

    [Header("Visual Feedback")]
    public GameObject deathParticlePrefab;
    public HealthBar healthBar;

    // Runtime
    protected float currentHealth;
    protected float currentSpeed;
    protected int waypointIndex = 0;
    protected SpriteRenderer sr;
    private string poolTag;
    private bool isDead = false;
    private Coroutine flashRoutine;

    // Slow state
    private float slowTimer = 0f;
    private float slowMultiplier = 1f;

    // Events
    public event System.Action OnDied;

    // IPoolable
    public void OnCreated(string tag) => poolTag = tag;

    public virtual void OnSpawn()
    {
        currentHealth = maxHealth;
        currentSpeed = moveSpeed;
        waypointIndex = 0;
        isDead = false;
        slowTimer = 0f;
        slowMultiplier = 1f;
        if (healthBar) healthBar.SetFill(1f);
        if (sr) sr.color = Color.white;
    }

    public void OnDespawn() { }

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    protected virtual void Update()
    {
        HandleSlow();
        Move();
    }

    protected virtual void Move()
    {
        var waypoints = WaypointManager.Instance.waypoints;
        if (waypointIndex >= waypoints.Length) { Leak(); return; }

        Transform target = waypoints[waypointIndex];
        float effectiveSpeed = currentSpeed * slowMultiplier;
        transform.position = Vector2.MoveTowards(
            transform.position, target.position, effectiveSpeed * Time.deltaTime);

        // Flip sprite based on movement direction
        Vector2 dir = (target.position - transform.position);
        if (Mathf.Abs(dir.x) > 0.01f)
            sr.flipX = dir.x < 0;

        if (Vector2.Distance(transform.position, target.position) < 0.05f)
            waypointIndex++;
    }

    void HandleSlow()
    {
        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0) slowMultiplier = 1f;
        }
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        float fill = Mathf.Clamp01(currentHealth / maxHealth);
        if (healthBar) healthBar.SetFill(fill);

        // Flash white on hit
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashHit());

        // Floating damage number
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.4f,
            Mathf.RoundToInt(amount).ToString(),
            Color.white);

        if (currentHealth <= 0) Die();
    }

    IEnumerator FlashHit()
    {
        sr.color = Color.white;
        // Quick flash to red, then back
        float t = 0;
        while (t < 0.08f)
        {
            sr.color = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), t / 0.08f);
            t += Time.deltaTime;
            yield return null;
        }
        t = 0;
        while (t < 0.08f)
        {
            sr.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), Color.white, t / 0.08f);
            t += Time.deltaTime;
            yield return null;
        }
        sr.color = Color.white;
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        GameManager.Instance.EarnGold(reward);

        // Floating gold number
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.7f,
            $"+{reward}g", Color.yellow);

        // Death particles
        if (deathParticlePrefab)
            ObjectPool.Instance.Spawn("DeathParticle", transform.position, Quaternion.identity);

        OnDied?.Invoke();
        ObjectPool.Instance.Despawn(poolTag, gameObject);
    }

    protected virtual void Leak()
    {
        if (isDead) return;
        isDead = true;
        GameManager.Instance.LoseLife(liveDamage);
        OnDied?.Invoke();
        ObjectPool.Instance.Despawn(poolTag, gameObject);
    }

    public void ApplySlow(float factor, float duration)
    {
        slowMultiplier = 1f - Mathf.Clamp01(factor);
        slowTimer = duration;
        // Tint blue when slowed
        sr.color = new Color(0.4f, 0.7f, 1f);
    }

    public int  GetWaypointIndex() => waypointIndex;
    public float GetHealth()       => currentHealth;
    public void  SetWaypointIndex(int i) => waypointIndex = i;
}