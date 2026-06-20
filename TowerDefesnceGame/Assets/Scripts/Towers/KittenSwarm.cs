using UnityEngine;

public class KittenSwarm : Tower
{
    [Header("Kitten Swarm")]
    public string bulletTag   = "KittenBall"; // tiny ball projectile
    public int    bulletsPerShot = 3;
    public float  spreadAngle = 15f;

    [Header("Visuals")]
    public Transform[] kittenPositions; // child transforms for 3 kitten sprites

    protected override void Shoot()
    {
        if (currentTarget == null) return;

        Vector2 baseDir = (currentTarget.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float spread = (i - bulletsPerShot / 2f) * (spreadAngle / bulletsPerShot);
            float angle  = baseAngle + spread;
            Quaternion rot = Quaternion.Euler(0, 0, angle);

            var obj = ObjectPool.Instance.Spawn(bulletTag, transform.position, rot);
            if (obj == null) continue;
            var b = obj.GetComponent<Bullet>();
            b?.Setup(currentTarget, Damage);
        }

        // Animate kittens — small bounce
        StartCoroutine(BounceKittens());
    }

    System.Collections.IEnumerator BounceKittens()
    {
        if (kittenPositions == null) yield break;
        foreach (var kp in kittenPositions)
        {
            if (kp == null) continue;
            Vector3 orig = kp.localPosition;
            kp.localPosition = orig + Vector3.up * 0.1f;
            yield return new WaitForSeconds(0.05f);
            kp.localPosition = orig;
        }
    }
}