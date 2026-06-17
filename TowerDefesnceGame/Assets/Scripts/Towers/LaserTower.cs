using UnityEngine;

public class LaserTower : Tower
{
    [Header("Laser")]
    public LineRenderer laserLine;
    public float damagePerSecond = 50f;
    public ParticleSystem hitParticles;

    protected override void Update()
    {
        FindTargetPublic();

        if (currentTarget != null)
        {
            FireLaser();
        }
        else
        {
            StopLaser();
        }
    }

    // Expose FindTarget since it's private in base — call via method
    void FindTargetPublic()
    {
        // Re-search manually since base.FindTarget is private
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Range, LayerMask.GetMask("Enemy"));
        Enemy best = null;
        float bestVal = float.MinValue;
        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e == null) continue;
            float v = e.GetWaypointIndex();
            if (v > bestVal) { bestVal = v; best = e; }
        }
        currentTarget = best != null ? best.transform : null;
    }

    void FireLaser()
    {
        if (!laserLine.enabled) laserLine.enabled = true;
        laserLine.SetPosition(0, transform.position);
        laserLine.SetPosition(1, currentTarget.position);

        if (hitParticles && !hitParticles.isPlaying)
        {
            hitParticles.transform.position = currentTarget.position;
            hitParticles.Play();
        }
        if (hitParticles) hitParticles.transform.position = currentTarget.position;

        currentTarget.GetComponent<Enemy>()?.TakeDamage(damagePerSecond * Time.deltaTime);
    }

    void StopLaser()
    {
        laserLine.enabled = false;
        if (hitParticles && hitParticles.isPlaying) hitParticles.Stop();
    }

    protected override void Shoot() { } // Unused — laser fires in Update
}