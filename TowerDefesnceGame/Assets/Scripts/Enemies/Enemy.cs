
using UnityEngine;
using System.Collections;
 
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour, IPoolable
{
    [Header("Stats")]
    public float maxHealth  = 100f;
    public float moveSpeed  = 2f;
    public int   reward     = 10;
    public int   liveDamage = 1;
 
    [Header("Visual Feedback")]
    public GameObject deathParticlePrefab;
    public HealthBar  healthBar;
 
    [Header("Elite Variant")]
    public bool  canBeElite       = true;
    public float eliteChance      = 0.1f;
    public float eliteHealthMult  = 2f;
    public float eliteSpeedMult   = 1.3f;
    public int   eliteRewardMult  = 3;
    public Color eliteColor       = new Color(1f, 0.8f, 0f); // gold tint
 
    // Runtime
    protected float currentHealth;
    protected float currentSpeed;
    protected int   waypointIndex = 0;
    protected SpriteRenderer sr;
    protected string poolTag;
 
    private bool      isDead       = false;
    private Coroutine flashRoutine;
 
    // Slow / wrap state
    private float slowTimer      = 0f;
    private float slowMultiplier = 1f;
    public  bool  IsSlowed       => slowTimer > 0f;
    public  bool  IsWrapped      { get; private set; }
    private float wrapTimer      = 0f;
 
    // Elite state
    public bool IsElite { get; private set; }
 
    // Events
    public event System.Action OnDied;
 
    // ── IPoolable ─────────────────────────────────────────────
 
    public void OnCreated(string tag) => poolTag = tag;
 
    public virtual void OnSpawn()
    {
        currentHealth  = maxHealth;
        currentSpeed   = moveSpeed;
        waypointIndex  = 0;
        isDead         = false;
        slowTimer      = 0f;
        slowMultiplier = 1f;
        IsWrapped      = false;
        wrapTimer      = 0f;
        IsElite        = false;
 
        if (healthBar) healthBar.SetFill(1f);
        if (sr) sr.color = Color.white;
 
        // Roll elite
        if (canBeElite && Random.value < eliteChance)
            BecomeElite();
 
        // Slide in from spawn point
        StartCoroutine(SlideIn());
    }
 
    public void OnDespawn() { }
 
    // ── Lifecycle ─────────────────────────────────────────────
 
    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
 
    protected virtual void Update()
    {
        HandleSlow();
        HandleWrap();
        Move();
    }
 
    // ── Slide-In Spawn Animation ──────────────────────────────
 
    IEnumerator SlideIn()
    {
        // Start slightly off-screen to the left of spawn
        Vector3 target  = transform.position;
        Vector3 startPos = target + Vector3.left * 1.5f;
        transform.position = startPos;
 
        float t = 0f;
        while (t < 0.25f)
        {
            transform.position = Vector3.Lerp(startPos, target, t / 0.25f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
    }
 
    // ── Elite ─────────────────────────────────────────────────
 
    void BecomeElite()
    {
        IsElite       = true;
        maxHealth    *= eliteHealthMult;
        currentHealth = maxHealth;
        moveSpeed    *= eliteSpeedMult;
        currentSpeed  = moveSpeed;
        reward       *= eliteRewardMult;
 
        if (sr) sr.color = eliteColor;
 
        // Scale up slightly
        transform.localScale *= 1.25f;
 
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.8f, "ELITE!", eliteColor);
    }
 
    // ── Movement ──────────────────────────────────────────────
 
    protected virtual void Move()
    {
        var waypoints = WaypointManager.Instance.waypoints;
        if (waypointIndex >= waypoints.Length) { Leak(); return; }
 
        Transform target = waypoints[waypointIndex];
      /*  float effectiveSpeed = currentSpeed * slowMultiplier
                             * LevelThemeManager.EnemySpeedMult;*/
                float effectiveSpeed = currentSpeed * slowMultiplier
    * LevelThemeManager.EnemySpeedMult
    * (WeatherSystem.Instance?.EnemySpeedModifier ?? 1f);
             
        transform.position = Vector2.MoveTowards(
            transform.position, target.position, effectiveSpeed * Time.deltaTime);
 
        Vector2 dir = target.position - transform.position;
        if (Mathf.Abs(dir.x) > 0.01f) sr.flipX = dir.x < 0;
 
        if (Vector2.Distance(transform.position, target.position) < 0.05f)
            waypointIndex++;
    }
 
    // ── Slow / Wrap ───────────────────────────────────────────
 
    void HandleSlow()
    {
        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
            {
                slowMultiplier = 1f;
                if (!IsWrapped && sr) sr.color = IsElite ? eliteColor : Color.white;
            }
        }
    }
 
    void HandleWrap()
    {
        if (IsWrapped)
        {
            wrapTimer -= Time.deltaTime;
            if (wrapTimer <= 0)
            {
                IsWrapped      = false;
                slowMultiplier = 1f;
                if (sr) sr.color = IsElite ? eliteColor : Color.white;
            }
        }
    }
 
    public void ApplySlow(float factor, float duration)
    {
        slowMultiplier = 1f - Mathf.Clamp01(factor);
        slowTimer      = duration;
        if (sr && !IsWrapped)
            sr.color = new Color(0.4f, 0.7f, 1f);
    }
 
    public void ApplyWrap(float duration)
    {
        IsWrapped      = true;
        wrapTimer      = duration;
        slowMultiplier = 0.001f;
        if (sr) sr.color = new Color(1f, 0.6f, 0.9f);
    }
 
    // ── Damage ────────────────────────────────────────────────
 
    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
 
        float fill = Mathf.Clamp01(currentHealth / maxHealth);
        if (healthBar) healthBar.SetFill(fill);
 
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashHit());
 
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.4f,
            Mathf.RoundToInt(amount).ToString(),
            Color.white);
 
        GetComponent<EnemyHurtSounds>()?.PlayHurtSound();
        GetComponent<EnemyAnimator>()?.PlayHurt();
 
        // Hit stop on big damage
        if (amount >= 80f)
            GameManager.Instance?.HitStop(0.06f);
        else if (amount >= 40f)
            GameManager.Instance?.HitStop(0.03f);
 
        // Shake scales with damage
        float shakeAmt = Mathf.Clamp(amount / 200f, 0.02f, 0.25f);
        CameraShake.Instance?.Shake(shakeAmt, 0.15f);
 
        if (currentHealth <= 0) Die();
    }
 
    IEnumerator FlashHit()
    {
        sr.color = Color.white;
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
        if (!IsSlowed && !IsWrapped)
            sr.color = IsElite ? eliteColor : Color.white;
    }
 
    // ── Death / Leak ──────────────────────────────────────────
 
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
 
        int goldAmount = Mathf.RoundToInt(reward *
            (1f + (RelicSystem.Instance?.TotalGoldBonus ?? 0f)));
        GameManager.Instance.EarnGold(goldAmount);
 
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.7f,
            $"+{goldAmount}g", Color.yellow);
 
        // Gold coin bounce to HUD
        var coin = ObjectPool.Instance.Spawn("GoldCoin",
            transform.position, Quaternion.identity);
        if (coin != null && UIManager.Instance != null)
        {
            Vector3 hudPos = UIManager.Instance.goldText.transform.position;
            coin.GetComponent<GoldCoinBounce>()?.Launch(transform.position, hudPos);
        }
 
        if (deathParticlePrefab)
            ObjectPool.Instance.Spawn("DeathParticle", transform.position, Quaternion.identity);
 
        GetComponent<EnemyHurtSounds>()?.PlayDeathSound();
        GetComponent<EnemyAnimator>()?.PlayDeath();
 
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
 
    // ── Accessors ─────────────────────────────────────────────
 
    public int   GetWaypointIndex()      => waypointIndex;
    public float GetHealth()             => currentHealth;
    public void  SetWaypointIndex(int i) => waypointIndex = i;
}
 