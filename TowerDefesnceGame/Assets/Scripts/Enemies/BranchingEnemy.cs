
using UnityEngine;
 
public class BranchingEnemy : Enemy
{
    private Transform[] branchWaypoints;
    private int         branchIndex  = 0;
    private bool        usingBranch  = false;
    private bool        rejoinedMain = false;
 
    public override void OnSpawn()
    {
        base.OnSpawn();
        usingBranch  = false;
        rejoinedMain = false;
        branchIndex  = 0;
        branchWaypoints = null;
    }
 
    protected override void Move()
    {
        var mgr = PathBranchingManager.Instance;
        var mainWPs = WaypointManager.Instance.waypoints;
 
        // Check if we've hit the branch point
        if (!usingBranch && !rejoinedMain && mgr != null
            && waypointIndex == mgr.branchPointIndex)
        {
            branchWaypoints = mgr.GetBranchForEnemy(this);
            usingBranch     = branchWaypoints != null && branchWaypoints.Length > 0;
            branchIndex     = 0;
        }
 
        Transform target;
 
        if (usingBranch && branchIndex < branchWaypoints.Length)
        {
            target = branchWaypoints[branchIndex];
            transform.position = Vector2.MoveTowards(
                transform.position, target.position, currentSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, target.position) < 0.05f)
            {
                branchIndex++;
                if (branchIndex >= branchWaypoints.Length)
                {
                    // Rejoin main path
                    usingBranch     = false;
                    rejoinedMain    = true;
                    waypointIndex   = mgr.rejoinIndex;
                }
            }
        }
        else
        {
            // Use base movement on main path
            base.Move();
        }
    }
}
 