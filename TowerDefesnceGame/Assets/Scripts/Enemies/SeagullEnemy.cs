using UnityEngine;

public class SeagullEnemy : FlyingEnemy
{
    [Header("Seagull Dive")]
    public float diveSpeed    = 8f;
    public bool  hasDived     = false;

    // Seagulls use the FlyingEnemy base but move faster
    // and drop straight to the end point in a dive

    public override void OnSpawn()
    {
        base.OnSpawn();
        moveSpeed = diveSpeed;
        hasDived  = false;
    }
}