using UnityEngine;

public class RCCarEnemy : Enemy
{
    [Header("RC Car")]
    public float zigzagAmount    = 0.8f;   // how far it swerves
    public float zigzagFrequency = 3f;     // swerves per second
    public float zigzagTimer     = 0f;

    protected override void Move()
    {
        var waypoints = WaypointManager.Instance.waypoints;
        if (waypointIndex >= waypoints.Length) { Leak(); return; }

        Transform target = waypoints[waypointIndex];
        Vector2 toTarget = (target.position - transform.position).normalized;

        // Perpendicular zigzag
        zigzagTimer += Time.deltaTime * zigzagFrequency;
        Vector2 perp = new Vector2(-toTarget.y, toTarget.x);
        Vector2 swerve = perp * Mathf.Sin(zigzagTimer) * zigzagAmount;

        Vector2 moveDir = (toTarget + swerve).normalized;
        transform.position += (Vector3)(moveDir * currentSpeed * Time.deltaTime);

        if (sr) sr.flipX = moveDir.x < 0;

        if (Vector2.Distance(transform.position, target.position) < 0.15f)
            waypointIndex++;
    }
}