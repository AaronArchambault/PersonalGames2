using UnityEngine;

public class BasicTower : Tower
{
    public Transform firePoint;
    public string bulletPoolTag = "Bullet";

    protected override void Shoot()
    {
        if (currentTarget == null) return;
        var obj = ObjectPool.Instance.Spawn(bulletPoolTag, firePoint.position, Quaternion.identity);
        if (obj == null) return;
        var b = obj.GetComponent<Bullet>();
        b?.Setup(currentTarget, Damage);
    }
}