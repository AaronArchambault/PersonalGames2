using UnityEngine;
using System.Collections;

public class GrumpyCat : Tower
{
    [Header("Grumpy Aura")]
    public float auraSlowFactor  = 0.35f;  // 35% slow
    public float auraDuration    = 1.5f;
    public float auraTickRate    = 0.5f;   // how often to reapply the debuff
    public float auraDamagePerTick = 5f;

    [Header("Grump Blast")]
    public float blastCooldown   = 5f;
    public float blastRange      = 3f;
    public float blastDamage     = 40f;
    public string blastEffectTag = "Explosion";

    [Header("Visuals")]
    public ParticleSystem grumpParticles;  // dark cloud particles around cat
    public SpriteRenderer faceSprite;      // swap between grumpy face expressions

    private float blastTimer = 0f;
    private float tickTimer  = 0f;

    protected override void Start()
    {
        base.Start();
        grumpParticles?.Play();
        StartCoroutine(ExpressionCycle());
    }

    protected override void Update()
    {
        base.Update();

        tickTimer += Time.deltaTime;
        if (tickTimer >= auraTickRate)
        {
            tickTimer = 0f;
            ApplyGrumpAura();
        }

        blastTimer += Time.deltaTime;
        if (blastTimer >= blastCooldown)
        {
            blastTimer = 0f;
            GrumpBlast();
        }
    }

    protected override void Shoot() { } // Grump uses its own timing

    void ApplyGrumpAura()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, Range, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            var e = hit.GetComponent<Enemy>();
            if (e == null) continue;
            e.ApplySlow(auraSlowFactor, auraDuration);
            e.TakeDamage(auraDamagePerTick);
        }
    }

    void GrumpBlast()
    {
        // Occasional big blast of pure grump energy
        ObjectPool.Instance.Spawn(blastEffectTag, transform.position, Quaternion.identity);
        CameraShake.Instance?.Shake(0.1f, 0.2f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, blastRange, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
            hit.GetComponent<Enemy>()?.TakeDamage(blastDamage + Damage);

        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.5f, "NO.", Color.grey);
    }

    IEnumerator ExpressionCycle()
    {
        // Cycle through grumpy faces (if you have sprite sheets)
        // For now just changes color to show grumpiness level
        while (true)
        {
            yield return new WaitForSeconds(2f);
            if (faceSprite)
                faceSprite.color = new Color(
                    Random.Range(0.6f, 1f), 0.3f, 0.3f);
            yield return new WaitForSeconds(0.2f);
            if (faceSprite) faceSprite.color = Color.white;
        }
    }
}