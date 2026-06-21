
using UnityEngine;
using System.Collections;
 
public class TreasureMouse : Enemy
{
    [Header("Treasure Mouse")]
    public int    treasureReward   = 100;
    public float  warningDistance  = 3f;
    public string sparkleEffectTag = "HitEffect";
 
    [Header("Visuals")]
    public ParticleSystem goldSparkles;
    public SpriteRenderer glowSprite;
 
    private bool announced = false;
 
    public override void OnSpawn()
    {
        base.OnSpawn();
        reward    = treasureReward;
        announced = false;
        goldSparkles?.Play();
        if (glowSprite) glowSprite.enabled = true;
        if (sr) sr.color = Color.yellow;
 
        UIManager.Instance?.Announce(
            "TREASURE MOUSE SPOTTED! Catch it!", Color.yellow);
        AudioManager.Instance?.Play("treasure_spawn");
    }
 
    protected override void Update()
    {
        base.Update();
 
        var waypoints = WaypointManager.Instance.waypoints;
        if (!announced && waypoints.Length > 0)
        {
            float distToEnd = Vector2.Distance(
                transform.position,
                waypoints[waypoints.Length - 1].position);
 
            if (distToEnd < warningDistance)
            {
                announced = true;
                UIManager.Instance?.Announce(
                    "TREASURE MOUSE ESCAPING!", Color.red);
                moveSpeed *= 1.5f;
            }
        }
    }
 
    protected override void Die()
    {
        for (int i = 0; i < 5; i++)
            ObjectPool.Instance.Spawn(
                sparkleEffectTag,
                transform.position + (Vector3)Random.insideUnitCircle * 0.5f,
                Quaternion.identity);
 
        UIManager.Instance?.Announce(
            $"TREASURE MOUSE CAUGHT! +{treasureReward}g!", Color.yellow);
        CameraShake.Instance?.Shake(0.2f, 0.3f);
        AudioManager.Instance?.Play("treasure_caught");
        base.Die();
    }
 
    protected override void Leak()
    {
        UIManager.Instance?.Announce(
            "Treasure Mouse escaped! No bonus.", Color.grey);
        AudioManager.Instance?.Play("btn_deny");
 
        // poolTag is now accessible because Enemy changed it to protected
        ObjectPool.Instance.Despawn(poolTag, gameObject);
    }
}