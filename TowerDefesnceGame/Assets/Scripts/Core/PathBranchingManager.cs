
using UnityEngine;
 
public class PathBranchingManager : MonoBehaviour
{
    public static PathBranchingManager Instance { get; private set; }
 
    [Header("Branch Waypoints")]
    [Tooltip("Index in WaypointManager where the path splits")]
    public int branchPointIndex = 3;
 
    [Tooltip("Path A waypoints after the branch")]
    public Transform[] pathA;
 
    [Tooltip("Path B waypoints after the branch")]
    public Transform[] pathB;
 
    [Tooltip("Both paths rejoin at this index in the main path")]
    public int rejoinIndex = 7;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
 
    // Called by Enemy when it reaches the branch point waypoint
    // Returns which waypoint array to switch to
    public Transform[] GetBranchForEnemy(Enemy e)
    {
        // Fast enemies prefer shorter path, slow enemies don't care
        bool preferShort = e.moveSpeed > 3f;
        bool chooseA = preferShort
            ? (pathA.Length < pathB.Length)
            : (Random.value > 0.5f);
 
        return chooseA ? pathA : pathB;
    }
}
 
// In Enemy.Move() — detect branch point and switch path:
// Add this check inside Move() after reaching a waypoint:
//
// if (waypointIndex == PathBranchingManager.Instance?.branchPointIndex)
// {
//     var branch = PathBranchingManager.Instance.GetBranchForEnemy(this);
//     if (branch != null && branch.Length > 0)
//     {
//         branchWaypoints = branch;
//         usingBranch = true;
//         branchIndex = 0;
//     }
// }
// This requires adding branchWaypoints/branchIndex fields to Enemy and
// checking usingBranch in Move() to use the alternate array.
// See BranchingEnemy.cs for a ready-made subclass approach.