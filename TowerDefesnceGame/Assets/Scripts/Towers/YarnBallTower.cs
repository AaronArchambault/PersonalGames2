using UnityEngine;
using System.Collections;

public class YarnBallTower : Tower
{
    [Header("Yarn")]
    public string yarnBulletTag = "YarnBall";
    public float  wrapDuration  = 1.5f;   // how long enemy is fully stopped
    public float  slowAfterWrap = 0.5f;   // slow applied after wrap ends

    protected override void Shoot()
    {
        if (currentTarget == null) return;
        var obj = ObjectPool.Instance.Spawn(yarnBulletTag,
            transform.position, Quaternion.identity);
        if (obj == null) return;
        var b = obj.GetComponent<YarnBall>();
        b?.Setup(currentTarget, Damage, wrapDuration, slowAfterWrap);
    }
}