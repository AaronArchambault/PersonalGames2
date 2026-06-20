using UnityEngine;
using System.Collections;

public class PrinterCat : Tower
{
    [Header("Printer Panic")]
    public float panicRadius     = 2.5f;
    public float knockbackWaypoints = 4;
    public float panicDamage     = 30f;
    public float panicCooldown   = 6f;
    public string panicEffectTag = "Explosion";

    [Header("Paper Projectile")]
    public string paperBulletTag = "Bullet";

    private float panicTimer = 0f;

    protected override void Update()
    {
        base.Update();

        panicTimer += Time.deltaTime;
        if (panicTimer >= panicCooldown)
        {
            panicTimer = 0f;
            StartCoroutine(PrinterPanic());
        }
    }

    protected override void Shoot()
    {
        // Regular shot — launches crumpled paper balls
        if (currentTarget == null) return;
        var obj = ObjectPool.Instance.Spawn(paperBulletTag,
            transform.position, Quaternion.identity);
        obj?.GetComponent<Bullet>()?.Setup(currentTarget, Damage);
    }

    IEnumerator PrinterPanic()
    {
        // BRRRRR sound of printer
        AudioManager.Instance?.Play("printer_sound");
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up, "PRINTER NOISE!!!", Color.red);
        CameraShake.Instance?.Shake(0.2f, 0.5f);

        // Cat absolutely loses it — knock ALL nearby enemies flying backward
        yield return new WaitForSeconds(0.3f);

        ObjectPool.Instance.Spawn(panicEffectTag, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, panicRadius, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            var e = hit.GetComponent<Enemy>();
            if (e == null) continue;
            e.TakeDamage(panicDamage + Damage);
            int newWP = Mathf.Max(0,
                e.GetWaypointIndex() - (int)knockbackWaypoints);
            e.SetWaypointIndex(newWP);
            if (newWP < WaypointManager.Instance.waypoints.Length)
                e.transform.position =
                    WaypointManager.Instance.waypoints[newWP].position;
        }

        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.5f, "PANIC ATTACK!", Color.red);
    }
}