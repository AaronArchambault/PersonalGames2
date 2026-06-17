using UnityEngine;

public class SplittingEnemy : Enemy
{
    public string childPoolTag = "FastEnemy";
    public int splitCount = 2;

    protected override void Die()
    {
        for (int i = 0; i < splitCount; i++)
        {
            Vector3 offset = (Vector3)Random.insideUnitCircle * 0.4f;
            var child = ObjectPool.Instance.Spawn(childPoolTag,
                transform.position + offset, Quaternion.identity);
            if (child == null) continue;
            var e = child.GetComponent<Enemy>();
            if (e != null) e.SetWaypointIndex(waypointIndex);
        }
        base.Die();
    }
}