
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;
 
public abstract class Tower : MonoBehaviour
{
    public enum TargetMode { First, Last, Strongest, Closest }
 
    [Header("Base Stats")]
    public float baseRange    = 3f;
    public float baseFireRate = 1f;
    public float baseDamage   = 25f;
    public int   cost         = 100;
 
    [Header("Upgrades")]
    public TowerUpgradeData upgradeData;
    public int pathALevel = 0;
    public int pathBLevel = 0;
 
    [Header("Targeting")]
    public TargetMode targetMode = TargetMode.First;
 
    [Header("Visual")]
    public Transform     turretPivot;
    public GameObject    rangeIndicator;
    public TextMeshPro   killCounterText;  // 3D TMP child above tower
    public SpriteRenderer baseSpriteRenderer;
 
    [Header("Personality Sounds")]
    public string idleSound     = "";  // e.g. "purr", "grumble"
    public float  idleSoundInterval = 8f;
 
    [Header("Synergy")]
    public float synergyBonusPercent = 0.1f;  // 10% bonus per adjacent same-type tower
    public float synergyRadius       = 1.5f;
 
    [HideInInspector] public int killCount = 0;
 
    // Runtime stats
    protected float Range;
    protected float FireRate;
    protected float Damage;
 
    protected Transform currentTarget;
    private float fireCooldown    = 0f;
    private bool  isSelected      = false;
    private float idleSoundTimer  = 0f;
    private float synergyDamageBonus = 0f;
    private float synergyRangeBonus  = 0f;
 
    // ── Lifecycle ─────────────────────────────────────────────
 
    protected virtual void Start()
    {
        RecalculateStats();
        RecalculateSynergy();
        if (rangeIndicator) rangeIndicator.SetActive(false);
        if (killCounterText) killCounterText.text = "";
        idleSoundTimer = Random.Range(0f, idleSoundInterval);
    }
 
    protected virtual void Update()
    {
        fireCooldown   -= Time.deltaTime;
        idleSoundTimer -= Time.deltaTime;
 
        FindTarget();
        RotateToTarget();
 
        if (currentTarget != null && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / FireRate;
            StartCoroutine(PulseRangeRing());
        }
 
        // Personality idle sound
        if (idleSoundTimer <= 0f && !string.IsNullOrEmpty(idleSound))
        {
            idleSoundTimer = idleSoundInterval + Random.Range(-2f, 2f);
            if (currentTarget == null) // only when idle (no enemies)
                AudioManager.Instance?.Play(idleSound);
        }
    }
 
    // ── Targeting ─────────────────────────────────────────────
 
    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, Range, LayerMask.GetMask("Enemy"));
 
        Enemy best      = null;
        float bestValue = float.MinValue;
 
        foreach (var hit in hits)
        {
            var e = hit.GetComponent<Enemy>();
            if (e == null) continue;
            float value = targetMode switch
            {
                TargetMode.First     =>  e.GetWaypointIndex(),
                TargetMode.Last      => -e.GetWaypointIndex(),
                TargetMode.Strongest => -e.GetHealth(),
                TargetMode.Closest   => -Vector2.Distance(transform.position, e.transform.position),
                _                    => 0
            };
            if (value > bestValue) { bestValue = value; best = e; }
        }
        currentTarget = best != null ? best.transform : null;
    }
 
    void RotateToTarget()
    {
        if (turretPivot == null || currentTarget == null) return;
        Vector2 dir   = currentTarget.position - turretPivot.position;
        float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        turretPivot.rotation = Quaternion.Lerp(
            turretPivot.rotation,
            Quaternion.Euler(0, 0, angle),
            Time.deltaTime * 15f);
    }
 
    protected abstract void Shoot();
 
    // ── Synergy ───────────────────────────────────────────────
 
    public void RecalculateSynergy()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(
            transform.position, synergyRadius, LayerMask.GetMask("Tower"));
 
        int sameTypeCount = 0;
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (col.GetComponent(GetType()) != null) sameTypeCount++;
        }
 
        synergyDamageBonus = baseDamage * synergyBonusPercent * sameTypeCount;
        synergyRangeBonus  = baseRange  * synergyBonusPercent * sameTypeCount * 0.5f;
 
        if (sameTypeCount > 0)
            FloatingTextPool.Instance?.Spawn(
                transform.position + Vector3.up * 0.8f,
                $"SYNERGY x{sameTypeCount}!", Color.cyan);
    }
 
    // ── Selection ─────────────────────────────────────────────
 
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (rangeIndicator) rangeIndicator.SetActive(selected);
    }
 
    public void CycleTargetMode()
    {
        int next    = ((int)targetMode + 1) % 4;
        targetMode  = (TargetMode)next;
    }
 
    // ── Range Ring Pulse ──────────────────────────────────────
 
    IEnumerator PulseRangeRing()
    {
        if (rangeIndicator == null || !isSelected) yield break;
        var ringsr = rangeIndicator.GetComponent<SpriteRenderer>();
        if (ringsr == null) yield break;
        Color orig = ringsr.color;
        ringsr.color = new Color(1f, 1f, 0f, 0.5f);
        yield return new WaitForSeconds(0.08f);
        ringsr.color = orig;
    }
 
    // ── Kill Count ────────────────────────────────────────────
 
    public void RegisterKill()
    {
        killCount++;
        if (killCounterText)
            killCounterText.text = killCount.ToString();
        MetagameManager.Instance?.AddKill(GetType().Name);
    }
 
    // ── Stats ─────────────────────────────────────────────────
 
    public void RecalculateStats()
    {
        Range    = baseRange;
        FireRate = baseFireRate;
        Damage   = baseDamage;
 
        if (upgradeData != null)
        {
            for (int i = 0; i < pathALevel && i < 3; i++)
            {
                Damage   += upgradeData.pathA[i].damageBonus;
                Range    += upgradeData.pathA[i].rangeBonus;
                FireRate += upgradeData.pathA[i].fireRateBonus;
            }
            for (int i = 0; i < pathBLevel && i < 3; i++)
            {
                Damage   += upgradeData.pathB[i].damageBonus;
                Range    += upgradeData.pathB[i].rangeBonus;
                FireRate += upgradeData.pathB[i].fireRateBonus;
            }
        }
 
        // Synergy bonus
        Damage += synergyDamageBonus;
        Range  += synergyRangeBonus;
 
        // Mother cat boost
        var motherBoost = GetComponent<MotherCatBoost>();
        if (motherBoost != null)
        {
            FireRate += motherBoost.fireRateBonus;
            Damage   += motherBoost.damageBonus;
        }
 
        // Relic system bonus
        if (RelicSystem.Instance != null)
        {
            Damage += RelicSystem.Instance.TotalDamageBonus;
            Range  += RelicSystem.Instance.TotalRangeBonus;
        }
 
        // Level theme multipliers
        Range    *= LevelThemeManager.TowerRangeMult;
        FireRate *= LevelThemeManager.TowerFireRateMult;
        Damage   *= LevelThemeManager.TowerDamageMult;
 
        // Basement lamp
        var lampStatus = GetComponent<BasementLampStatus>();
        if (lampStatus != null && !lampStatus.nearLamp)
            Range *= 0.5f;
 
        if (rangeIndicator)
            rangeIndicator.transform.localScale = Vector3.one * Range * 2f;
    }
 
    // ── Upgrades ──────────────────────────────────────────────
 
    public bool TryUpgrade(int path)
    {
        if (upgradeData == null) return false;
        int level = path == 0 ? pathALevel : pathBLevel;
        if (level >= 3) return false;
 
        int upgradeCost = path == 0
            ? upgradeData.pathA[level].cost
            : upgradeData.pathB[level].cost;
 
        if (!GameManager.Instance.SpendGold(upgradeCost)) return false;
 
        if (path == 0) pathALevel++; else pathBLevel++;
        RecalculateStats();
        StartCoroutine(UpgradePopAnim());
        GetComponent<TowerAnimator>()?.PlayAttack();
        return true;
    }
 
    public int GetUpgradeCost(int path)
    {
        if (upgradeData == null) return -1;
        int level = path == 0 ? pathALevel : pathBLevel;
        if (level >= 3) return -1;
        return path == 0
            ? upgradeData.pathA[level].cost
            : upgradeData.pathB[level].cost;
    }
 
    public int GetSellValue()
    {
        return Mathf.RoundToInt(cost * 0.6f) + killCount * 2;
    }
 
    // ── Power-Up Support ──────────────────────────────────────
 
    public void ResetFireCooldown() => fireCooldown = 0f;
 
    public void ApplyFireRateBoost(float multiplier, float duration)
    {
        StartCoroutine(FireRateBoostRoutine(multiplier, duration));
    }
 
    IEnumerator FireRateBoostRoutine(float multiplier, float duration)
    {
        FireRate *= multiplier;
        yield return new WaitForSeconds(duration);
        FireRate /= multiplier;
    }
 
    // ── Animations ────────────────────────────────────────────
 
    IEnumerator UpgradePopAnim()
    {
        Vector3 orig = transform.localScale;
        float t = 0f;
        while (t < 0.1f)
        {
            transform.localScale = Vector3.Lerp(orig, orig * 1.3f, t / 0.1f);
            t += Time.deltaTime; yield return null;
        }
        t = 0f;
        while (t < 0.1f)
        {
            transform.localScale = Vector3.Lerp(orig * 1.3f, orig, t / 0.1f);
            t += Time.deltaTime; yield return null;
        }
        transform.localScale = orig;
    }
 
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, baseRange);
    }
}