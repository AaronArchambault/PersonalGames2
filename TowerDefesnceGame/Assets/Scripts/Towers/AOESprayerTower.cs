using UnityEngine;

public class AOESprayerTower : Tower
{
    public Transform firePoint;
    public string bulletPoolTag = "Bullet";
    public int    bulletsPerShot = 6;
    public float  spreadAngle   = 50f;

    protected override void Shoot()
    {
        if (currentTarget == null) return;

        Vector2 baseDir = (currentTarget.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float a = baseAngle + Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
            Quaternion rot = Quaternion.Euler(0, 0, a);
            var obj = ObjectPool.Instance.Spawn(bulletPoolTag, firePoint.position, rot);
            if (obj == null) continue;
            var b = obj.GetComponent<Bullet>();
            b?.SetupDirectional(rot * Vector3.right, Damage);
        }
    }
}