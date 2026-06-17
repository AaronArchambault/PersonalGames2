using UnityEngine;

public class BombTower : Tower
{
    public Transform firePoint;
    public float explosionRadius = 1.8f;
    public string bulletPoolTag  = "BombShell";
    public string explosionPoolTag = "Explosion";

    protected override void Shoot()
    {
        if (currentTarget == null) return;
        var obj = ObjectPool.Instance.Spawn(bulletPoolTag, firePoint.position, Quaternion.identity);
        if (obj == null) return;
        var b = obj.GetComponent<BombShell>();
        b?.Setup(currentTarget.position, Damage, explosionRadius, explosionPoolTag);
    }
}