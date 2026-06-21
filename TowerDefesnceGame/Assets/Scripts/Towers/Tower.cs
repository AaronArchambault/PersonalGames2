
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
 
public abstract class Tower : MonoBehaviour
{
    // Enum declared before any [Header] attributes
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
 
    // ── Selection ─────────────────────────────────────────────
    // NOTE: OnMouseDown is intentionally removed.
    // Tower selection is handled entirely by TowerPlacer.OnClick()
    // which correctly checks for UI hits before selecting towers.
 
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (rangeIndicator) rangeIndicator.SetActive(selected);
    }
 
    // ── Stats ─────────────────────────────────────────────────
 
    public void RecalculateStats()
    {
        Range    = baseRange;
        FireRate = baseFireRate;
        Damage   = baseDamage;
 
        // Path A upgrades
        if (upgradeData != null)
        {
            for (int i = 0; i < pathALevel && i < 3; i++)
            {
                Damage   += upgradeData.pathA[i].damageBonus;
                Range    += upgradeData.pathA[i].rangeBonus;
                FireRate += upgradeData.pathA[i].fireRateBonus;
            }
            // Path B upgrades
            for (int i = 0; i < pathBLevel && i < 3; i++)
            {
                Damage   += upgradeData.pathB[i].damageBonus;
                Range    += upgradeData.pathB[i].rangeBonus;
                FireRate += upgradeData.pathB[i].fireRateBonus;
            }
        }
 
        // Mother cat boost (applied by MotherCatTower)
        var motherBoost = GetComponent<MotherCatBoost>();
        if (motherBoost != null)
        {
            FireRate += motherBoost.fireRateBonus;
            Damage   += motherBoost.damageBonus;
        }
 
        // Level theme multipliers
        Range    *= LevelThemeManager.TowerRangeMult;
        FireRate *= LevelThemeManager.TowerFireRateMult;
        Damage   *= LevelThemeManager.TowerDamageMult;
 
        // Basement lamp range reduction
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
 
        if (path == 0) pathALevel++;
        else           pathBLevel++;
 
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
        int baseValue = Mathf.RoundToInt(cost * 0.6f);
        int killBonus = killCount * 2;
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
        float t = 0f;
        while (t < 0.1f)
        {
            transform.localScale = Vector3.Lerp(orig, orig * 1.3f, t / 0.1f);
            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;
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