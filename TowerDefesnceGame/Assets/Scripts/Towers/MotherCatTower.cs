using UnityEngine;
using System.Collections.Generic;

public class MotherCatTower : Tower
{
    [Header("Mother Cat")]
    public float fireRateBoostPercent = 0.25f; // 25% fire rate boost
    public float damageBoostPercent   = 0.10f; // 10% damage boost
    public float boostRadius          = 3f;
    public ParticleSystem heartParticles;

    [Header("Feeding")]
    public float feedInterval = 8f;   // mother cat feeds nearby towers (burst heal)
    public int   feedGoldCost = 0;    // free

    private List<Tower> boostedTowers = new();
    private float feedTimer = 0f;

    protected override void Start()
    {
        base.Start();
        heartParticles?.Play();
        ApplyBoosts();
    }

    protected override void Update()
    {
        // Recheck boosted towers periodically
        feedTimer += Time.deltaTime;
        if (feedTimer >= 1f)
        {
            feedTimer = 0f;
            ApplyBoosts();
        }

        // Feeding burst
        if (feedTimer >= feedInterval)
            FeedNearbyTowers();
    }

    protected override void Shoot() { } // Mother cat doesn't attack

    void ApplyBoosts()
    {
        // Remove old boosts
        foreach (var t in boostedTowers)
            if (t != null) t.RecalculateStats();
        boostedTowers.Clear();

        // Find towers in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, boostRadius, LayerMask.GetMask("Tower"));

        foreach (var hit in hits)
        {
            var t = hit.GetComponent<Tower>();
            if (t == null || t == this) continue;

            // Apply boost by temporarily adding to base stats
            // We do this by adding a MotherCatBoost component
            var boost = hit.GetComponent<MotherCatBoost>();
            if (boost == null)
            {
                boost = hit.gameObject.AddComponent<MotherCatBoost>();
                boost.source = this;
            }
            boost.fireRateBonus = t.baseFireRate * fireRateBoostPercent;
            boost.damageBonus   = t.baseDamage   * damageBoostPercent;
            t.RecalculateStats();
            boostedTowers.Add(t);
        }
    }

    void FeedNearbyTowers()
    {
        foreach (var t in boostedTowers)
        {
            if (t == null) continue;
            FloatingTextPool.Instance?.Spawn(
                t.transform.position + Vector3.up, "FED!", Color.yellow);
        }
        heartParticles?.Play();
    }

    void OnDestroy()
    {
        // Remove all boosts when mother cat is sold/removed
        foreach (var t in boostedTowers)
        {
            if (t == null) continue;
            var boost = t.GetComponent<MotherCatBoost>();
            if (boost != null) Destroy(boost);
            t.RecalculateStats();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.8f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, boostRadius);
    }
}

// Attached to towers being boosted
public class MotherCatBoost : MonoBehaviour
{
    public MotherCatTower source;
    public float fireRateBonus;
    public float damageBonus;
}