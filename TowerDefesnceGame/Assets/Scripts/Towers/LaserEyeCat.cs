using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserEyeCat : Tower
{
    [Header("Laser Eye")]
    public LineRenderer primaryLaser;
    public LineRenderer chainLaser;      // second laser for chain
    public int   chainCount    = 2;      // how many enemies to chain to
    public float chainRange    = 1.5f;   // radius to find chain targets
    public float chainDamage   = 0.5f;   // chain does 50% of main damage

    [Header("Effects")]
    public ParticleSystem eyeGlowParticles;
    public string hitEffectTag = "HitEffect";

    private List<Enemy> chainTargets = new();

    protected override void Start()
    {
        base.Start();
        if (primaryLaser) primaryLaser.enabled = false;
        if (chainLaser)   chainLaser.enabled   = false;
    }

    protected override void Update()
    {
        base.Update(); // handles targeting and fire cooldown
        UpdateLasers();
    }

    void UpdateLasers()
    {
        if (currentTarget != null)
        {
            // Draw primary laser
            if (primaryLaser)
            {
                primaryLaser.enabled = true;
                primaryLaser.SetPosition(0, transform.position);
                primaryLaser.SetPosition(1, currentTarget.position);
            }
            if (eyeGlowParticles && !eyeGlowParticles.isPlaying)
                eyeGlowParticles.Play();
        }
        else
        {
            if (primaryLaser) primaryLaser.enabled = false;
            if (chainLaser)   chainLaser.enabled   = false;
            if (eyeGlowParticles && eyeGlowParticles.isPlaying)
                eyeGlowParticles.Stop();
        }
    }

    protected override void Shoot()
    {
        if (currentTarget == null) return;

        // Primary hit
        var primaryEnemy = currentTarget.GetComponent<Enemy>();
        primaryEnemy?.TakeDamage(Damage);
        ObjectPool.Instance.Spawn(hitEffectTag, currentTarget.position, Quaternion.identity);

        // Find chain targets near primary target
        chainTargets.Clear();
        Collider2D[] nearby = Physics2D.OverlapCircleAll(
            currentTarget.position, chainRange, LayerMask.GetMask("Enemy"));

        int chains = 0;
        foreach (var col in nearby)
        {
            if (chains >= chainCount) break;
            var e = col.GetComponent<Enemy>();
            if (e == null || e.transform == currentTarget) continue;
            chainTargets.Add(e);
            e.TakeDamage(Damage * chainDamage);
            chains++;
        }

        // Draw chain laser to first chain target
        if (chainLaser && chainTargets.Count > 0)
        {
            chainLaser.enabled = true;
            chainLaser.SetPosition(0, currentTarget.position);
            chainLaser.SetPosition(1, chainTargets[0].transform.position);
            StartCoroutine(HideChainLaser());
        }
    }

    IEnumerator HideChainLaser()
    {
        yield return new WaitForSeconds(0.1f);
        if (chainLaser) chainLaser.enabled = false;
    }
}