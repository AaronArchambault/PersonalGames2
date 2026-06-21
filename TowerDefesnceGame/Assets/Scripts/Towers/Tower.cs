using UnityEngine; 
using System.Collections;
 
public abstract class Tower : MonoBehaviour
{
    // Enum must be declared BEFORE any Header attributes
    // and with NO Header above it
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
    public Transform  turretPivot;
    public GameObject rangeIndicator;

    [HideInInspector] public int killCount = 0;

    // rest of the script stays exactly the same...

    // Runtime stats
    protected float Range;
    protected float FireRate;
    protected float Damage;
 
    protected Transform currentTarget;
    private float fireCooldown = 0f;
    private bool  isSelected   = false;
 
    // ── Lifecycle ─────────────────────────────────────────────
 
    protected virtual void Start()
    {
        RecalculateStats();
        if (rangeIndicator) rangeIndicator.SetActive(false);
    }
 
    protected virtual void Update()
    {
        fireCooldown -= Time.deltaTime;
        FindTarget();
        RotateToTarget();
 
        if (currentTarget != null && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / FireRate;
        }
    }
 
    // ── Targeting ─────────────────────────────────────────────
 
    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, Range, LayerMask.GetMask("Enemy"));
 
        Enemy best = null;
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
 
    // ── Selection ─────────────────────────────────────────────
 
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (rangeIndicator) rangeIndicator.SetActive(selected);
    }
 
    void OnMouseDown()
    {
        SetSelected(true);
        TowerUpgradeUI.Instance?.Show(this);
    }
 
    // ── Stats ─────────────────────────────────────────────────
 
    public void RecalculateStats()
    {
        Range    = baseRange;
        FireRate = baseFireRate;
        Damage   = baseDamage;
 
        // Path upgrades
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
 
        // Mother cat boost
        var motherBoost = GetComponent<MotherCatBoost>();
        if (motherBoost != null)
        {
            FireRate += motherBoost.fireRateBonus;
            Damage   += motherBoost.damageBonus;
        }
 
        // Level theme multipliers — safe if LevelThemeManager doesn't exist
        Range    *= LevelThemeManager.TowerRangeMult;
        FireRate *= LevelThemeManager.TowerFireRateMult;
        Damage   *= LevelThemeManager.TowerDamageMult;
 
        // Basement lamp check
        var lampStatus = GetComponent<BasementLampStatus>();
        if (lampStatus != null && !lampStatus.nearLamp)
            Range *= 0.5f;
 
        // Update range indicator circle size
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
        int baseValue  = Mathf.RoundToInt(cost * 0.6f);
        int killBonus  = killCount * 2;
        return baseValue + killBonus;
    }
 
    // ── Power-Up Support ──────────────────────────────────────
 
    public void ResetFireCooldown()
    {
        fireCooldown = 0f;
    }
 
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
        float t = 0;
        while (t < 0.1f)
        {
            transform.localScale = Vector3.Lerp(orig, orig * 1.3f, t / 0.1f);
            t += Time.deltaTime;
            yield return null;
        }
        t = 0;
        while (t < 0.1f)
        {
            transform.localScale = Vector3.Lerp(orig * 1.3f, orig, t / 0.1f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = orig;
    }
 
    // ── Gizmos ────────────────────────────────────────────────
 
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, baseRange);
    }
}


/*using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseRange    = 3f;
    public float baseFireRate = 1f;
    public float baseDamage   = 25f;
    public int   cost         = 100;

[HideInInspector] public int killCount = 0;

    [Header("Upgrades")]
    public TowerUpgradeData upgradeData;
    public int pathALevel = 0;
    public int pathBLevel = 0;

  //  [Header("Targeting")]
    public enum TargetMode { First, Last, Strongest, Closest }
    public TargetMode targetMode = TargetMode.First;

    [Header("Visual")]
    public Transform turretPivot;       // Rotates to face target
    public GameObject rangeIndicator;   // Circle shown on select

    // Runtime stats
    protected float Range;
    protected float FireRate;
    protected float Damage;

    protected Transform currentTarget;
    private float fireCooldown = 0f;
    private bool isSelected = false;

    protected virtual void Start()
    {
        RecalculateStats();
        if (rangeIndicator) rangeIndicator.SetActive(false);
    }

    protected virtual void Update()
    {
        fireCooldown -= Time.deltaTime;
        FindTarget();
        RotateToTarget();

        if (currentTarget != null && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / FireRate;
        }
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Range, LayerMask.GetMask("Enemy"));
        Enemy best = null;
        float bestValue = float.MinValue;

        foreach (var hit in hits)
        {
            var e = hit.GetComponent<Enemy>();
            if (e == null) continue;
            float value = targetMode switch
            {
                TargetMode.First     => e.GetWaypointIndex(),
                TargetMode.Last      => -e.GetWaypointIndex(),
                TargetMode.Strongest => -e.GetHealth(),
                TargetMode.Closest   => -Vector2.Distance(transform.position, e.transform.position),
                _ => 0
            };
            if (value > bestValue) { bestValue = value; best = e; }
        }
        currentTarget = best != null ? best.transform : null;
    }

    void RotateToTarget()
    {
        if (turretPivot == null || currentTarget == null) return;
        Vector2 dir = currentTarget.position - turretPivot.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        turretPivot.rotation = Quaternion.Lerp(
            turretPivot.rotation,
            Quaternion.Euler(0, 0, angle),
            Time.deltaTime * 15f);
    }

    protected abstract void Shoot();

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (rangeIndicator) rangeIndicator.SetActive(selected);
    }

    // --- Upgrade system ---
    public void RecalculateStats()
    {
        Range    = baseRange;
        FireRate = baseFireRate;
        Damage   = baseDamage;

        if (upgradeData == null) return;
        for (int i = 0; i < pathALevel && i < 3; i++)
        {
            var u = upgradeData.pathA[i];
            Damage   += u.damageBonus;
            Range    += u.rangeBonus;
            FireRate += u.fireRateBonus;
        }
        for (int i = 0; i < pathBLevel && i < 3; i++)
        {
            var u = upgradeData.pathB[i];
            Damage   += u.damageBonus;
            Range    += u.rangeBonus;
            FireRate += u.fireRateBonus;
        }

        // Update range indicator scale
        if (rangeIndicator)
            rangeIndicator.transform.localScale = Vector3.one * Range * 2f;


                var motherBoost = GetComponent<MotherCatBoost>();
                if (motherBoost != null)
                {
                     FireRate += motherBoost.fireRateBonus;
                    Damage   += motherBoost.damageBonus;
                } 


                // Apply level theme multipliers
Range    *= LevelThemeManager.TowerRangeMult;
FireRate *= LevelThemeManager.TowerFireRateMult;
Damage   *= LevelThemeManager.TowerDamageMult;

// Basement: near-lamp override
var lampStatus = GetComponent<BasementLampStatus>();
if (lampStatus != null && !lampStatus.nearLamp)
    Range *= 0.5f; // half range when not near a lamp in basement  
    }

    public bool TryUpgrade(int path)
    {
        if (upgradeData == null) return false;
        int level = path == 0 ? pathALevel : pathBLevel;
        if (level >= 3) return false;
        int c = path == 0 ? upgradeData.pathA[level].cost
                           : upgradeData.pathB[level].cost;
        if (!GameManager.Instance.SpendGold(c)) return false;
        if (path == 0) pathALevel++; else pathBLevel++;
        RecalculateStats();

        // Visual pop on upgrade
        StartCoroutine(UpgradePopAnim());
        return true;
    }

    System.Collections.IEnumerator UpgradePopAnim()
    {
        Vector3 orig = transform.localScale;
        float t = 0;
        while (t < 0.1f)
        {
            transform.localScale = Vector3.Lerp(orig, orig * 1.3f, t / 0.1f);
            t += Time.deltaTime;
            yield return null;
        }
        t = 0;
        while (t < 0.1f)
        {
            transform.localScale = Vector3.Lerp(orig * 1.3f, orig, t / 0.1f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = orig;
    }

    public int GetUpgradeCost(int path)
    {
        if (upgradeData == null) return -1;
        int level = path == 0 ? pathALevel : pathBLevel;
        if (level >= 3) return -1;
        return path == 0 ? upgradeData.pathA[level].cost
                         : upgradeData.pathB[level].cost;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, baseRange);
    }

    public void ResetFireCooldown()
{
    fireCooldown = 0f;
}

// Called by Speed Nap power-up
public void ApplyFireRateBoost(float multiplier, float duration)
{
    StartCoroutine(FireRateBoostRoutine(multiplier, duration));
}

System.Collections.IEnumerator FireRateBoostRoutine(float multiplier, float duration)
{
    FireRate *= multiplier;
    yield return new WaitForSeconds(duration);
    FireRate /= multiplier;
}
}*/