using UnityEngine;

public class SlowTower : Tower
{
    public Transform firePoint;
    [Range(0f, 0.9f)] public float slowFactor = 0.5f;
    public float slowDuration = 2.5f;
    public string bulletPoolTag = "SlowBullet";

    protected override void Shoot()
    {
        if (currentTarget == null) return;
        var obj = ObjectPool.Instance.Spawn(bulletPoolTag, firePoint.position, Quaternion.identity);
        if (obj == null) return;
        var b = obj.GetComponent<SlowBullet>();
        b?.Setup(currentTarget, slowFactor, slowDuration);
    }
}