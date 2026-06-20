using UnityEngine;

public class ChonkCat : Tower
{
    [Header("Chonk")]
    public float slowFactor   = 0.7f;   // 70% slow
    public float slowDuration = 3f;
    public float auraRange    = 2f;     // all enemies in this range get slowed

    [Header("Sit Attack")]
    public string bulletPoolTag = "ChonkSit";
    public float  sitDamage    = 15f;

    [Header("Visuals")]
    public ParticleSystem crumbParticles; // crumbs floating around the chonk

    protected override void Start()
    {
        base.Start();
        crumbParticles?.Play();
    }

    protected override void Update()
    {
        base.Update();
        ApplyAuraSlows();
    }

    void ApplyAuraSlows()
    {
        // Every frame, slow everything in aura range
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, auraRange, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
            hit.GetComponent<Enemy>()?.ApplySlow(slowFactor * 0.5f, 0.5f);
    }

    protected override void Shoot()
    {
        if (currentTarget == null) return;
        // Shoot a "sit" projectile — heavy slow bullet
        var obj = ObjectPool.Instance.Spawn(bulletPoolTag,
            transform.position, Quaternion.identity);
        if (obj == null) return;
        var b = obj.GetComponent<SlowBullet>();
        b?.Setup(currentTarget, slowFactor, slowDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, auraRange);
    }
}