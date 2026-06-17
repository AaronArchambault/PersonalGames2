using UnityEngine;

public class FlyingEnemy : Enemy
{
    [Header("Flying")]
    public float flyHeight = 0.3f; // bob amount
    private Vector3 basePos;
    private float bobTimer;

    public override void OnSpawn()
    {
        base.OnSpawn();
        basePos = transform.position;
        bobTimer = 0f;
    }

    protected override void Move()
    {
        var waypoints = WaypointManager.Instance.waypoints;
        if (waypointIndex >= waypoints.Length) { Leak(); return; }

        // Flying enemies ignore terrain — go straight
        Transform target = waypoints[waypointIndex];
        transform.position = Vector2.MoveTowards(
            transform.position, target.position, currentSpeed * Time.deltaTime);

        // Subtle bob
        bobTimer += Time.deltaTime * 3f;
        Vector3 pos = transform.position;
        pos.y += Mathf.Sin(bobTimer) * flyHeight * Time.deltaTime;
        transform.position = pos;

        Vector2 dir = target.position - transform.position;
        if (Mathf.Abs(dir.x) > 0.01f) sr.flipX = dir.x < 0;

        if (Vector2.Distance(transform.position, target.position) < 0.05f)
            waypointIndex++;
    }
}