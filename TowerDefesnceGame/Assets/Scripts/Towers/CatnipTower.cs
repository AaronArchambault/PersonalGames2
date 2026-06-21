using UnityEngine;
using System.Collections;

public class CatnipTower : Tower
{
    [Header("Catnip")]
    public float catnipSlowFactor  = 0.4f;
    public float catnipDuration    = 2f;
    public float loopyChance       = 0.3f;  // 30% chance enemy reverses direction
    public float tickRate          = 0.5f;
    public ParticleSystem catnipCloud;

    [Header("Loopy Effect")]
    public int loopyWaypointReverse = 2; // reverses this many waypoints

    private float tickTimer = 0f;

    protected override void Start()
    {
        base.Start();
        catnipCloud?.Play();
    }

    protected override void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickRate)
        {
            tickTimer = 0f;
            ApplyCatnip();
        }
    }

    protected override void Shoot() { } // Catnip uses its own tick system

    void ApplyCatnip()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, Range, LayerMask.GetMask("Enemy"));

        foreach (var hit in hits)
        {
            var e = hit.GetComponent<Enemy>();
            if (e == null) continue;

            // Apply slow
            e.ApplySlow(catnipSlowFactor, catnipDuration);
            e.TakeDamage(Damage * tickRate); // tiny tick damage

            // Random loopy reversal
            if (Random.value < loopyChance)
                StartCoroutine(MakeLoopy(e));
        }
    }

    IEnumerator MakeLoopy(Enemy e)
    {
        if (e == null) yield break;
        int newWP = Mathf.Max(0, e.GetWaypointIndex() - loopyWaypointReverse);
        e.SetWaypointIndex(newWP);
        FloatingTextPool.Instance?.Spawn(
            e.transform.position + Vector3.up, "LOOPY!", new Color(0.6f, 1f, 0.4f));

        // Tint green for catnip effect
        var sr = e.GetComponent<SpriteRenderer>();
        if (sr)
        {
            sr.color = new Color(0.6f, 1f, 0.4f);
            yield return new WaitForSeconds(catnipDuration);
            if (sr) sr.color = Color.white;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 1f, 0.3f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}