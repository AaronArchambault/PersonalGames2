using UnityEngine;

public class SniperTower : Tower
{
    public string muzzleFlashTag = "MuzzleFlash";
    public Transform firePoint;
    public LineRenderer tracerLine;

    protected override void Shoot()
    {
        if (currentTarget == null) return;
        currentTarget.GetComponent<Enemy>()?.TakeDamage(Damage);

        ObjectPool.Instance.Spawn(muzzleFlashTag, firePoint.position, Quaternion.identity);

        // Tracer line flash
        if (tracerLine) StartCoroutine(ShowTracer(currentTarget.position));
    }

    System.Collections.IEnumerator ShowTracer(Vector3 end)
    {
        tracerLine.enabled = true;
        tracerLine.SetPosition(0, firePoint.position);
        tracerLine.SetPosition(1, end);
        yield return new WaitForSeconds(0.05f);
        tracerLine.enabled = false;
    }
}