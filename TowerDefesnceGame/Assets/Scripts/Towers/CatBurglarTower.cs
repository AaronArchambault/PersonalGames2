using UnityEngine;

public class CatBurglarTower : Tower
{
    [Header("Cat Burglar")]
    public float goldMultiplier  = 1.5f;  // enemies in range give 50% more gold
    public float stealRadius     = 3f;    // range of the gold-stealing aura
    public string bulletTag      = "Bullet";

    [Header("Visuals")]
    public ParticleSystem coinParticles;

    protected override void Start()
    {
        base.Start();
        // Register all enemies entering range for gold bonus
        StartCoroutine(GoldAuraLoop());
    }

    protected override void Shoot()
    {
        if (currentTarget == null) return;
        var obj = ObjectPool.Instance.Spawn(bulletTag, transform.position, Quaternion.identity);
        obj?.GetComponent<Bullet>()?.Setup(currentTarget, Damage);
    }

    System.Collections.IEnumerator GoldAuraLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            TagNearbyEnemies();
        }
    }

    void TagNearbyEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, stealRadius, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            var e = hit.GetComponent<Enemy>();
            if (e == null) continue;
            // Subscribe to death only once using the tag system
            var tag = hit.GetComponent<BurglarTag>();
            if (tag != null) continue; // already tagged

            tag = hit.gameObject.AddComponent<BurglarTag>();
            tag.bonusGold = Mathf.RoundToInt(e.reward * (goldMultiplier - 1f));
            tag.coinParticles = coinParticles;
        }
    }
}

// Helper component — attached to enemies in burglar range
public class BurglarTag : MonoBehaviour
{
    public int bonusGold;
    public ParticleSystem coinParticles;

    void OnDisable()
    {
        // Enemy is dying/despawning — pay bonus gold
        if (bonusGold > 0)
        {
            GameManager.Instance?.EarnGold(bonusGold);
            FloatingTextPool.Instance?.Spawn(
                transform.position + Vector3.up, $"+{bonusGold}g STOLEN!", Color.yellow);
            coinParticles?.Play();
        }
        Destroy(this);
    }
}