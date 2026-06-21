using UnityEngine;
using System.Collections;

public class MainecoonSniper : Tower
{
    [Header("Mainecoon")]
    public string muzzleFlashTag = "MuzzleFlash";
    public Transform firePoint;
    public LineRenderer tracerLine;

    [Header("Meow")]
    public string[] meowSounds = { "meow_1", "meow_2", "meow_3" };
    public float meowPitchVariance = 0.2f;

    [Header("Penetration")]
    public bool  penetrating   = false; // Path B upgrade enables this
    public float penetrateRange = 6f;   // line from target, hits enemies behind

    protected override void Shoot()
    {
        if (currentTarget == null) return;

        // MEOW
        string meow = meowSounds[Random.Range(0, meowSounds.Length)];
        AudioManager.Instance?.Play(meow);

        // Instant hit
        currentTarget.GetComponent<Enemy>()?.TakeDamage(Damage);

        // Penetrating shot — hits enemies in a line behind the target
        if (penetrating)
        {
            Vector2 dir = (currentTarget.position - transform.position).normalized;
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                currentTarget.position, dir, penetrateRange,
                LayerMask.GetMask("Enemy"));
            foreach (var h in hits)
                h.collider.GetComponent<Enemy>()?.TakeDamage(Damage * 0.5f);
        }

        // Flash + tracer
        ObjectPool.Instance.Spawn(muzzleFlashTag,
            firePoint != null ? firePoint.position : transform.position,
            Quaternion.identity);

        if (tracerLine) StartCoroutine(ShowTracer(currentTarget.position));

        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.8f, "MEOW!", Color.white);
    }

    IEnumerator ShowTracer(Vector3 end)
    {
        if (tracerLine == null) yield break;
        tracerLine.enabled = true;
        tracerLine.SetPosition(0, firePoint != null ? firePoint.position : transform.position);
        tracerLine.SetPosition(1, end);
        yield return new WaitForSeconds(0.06f);
        tracerLine.enabled = false;
    }

    public void EnablePenetrating() => penetrating = true;
}