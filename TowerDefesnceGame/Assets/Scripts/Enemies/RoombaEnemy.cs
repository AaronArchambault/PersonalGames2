using UnityEngine;

public class RoombaEnemy : Enemy
{
    [Header("Roomba")]
    public float absorbChance = 0.3f;  // 30% chance to negate a hit entirely
    public ParticleSystem dirtParticles;

    public override void OnSpawn()
    {
        base.OnSpawn();
        dirtParticles?.Play();
    }

    public override void TakeDamage(float amount)
    {
        // Sometimes just absorbs the hit
        if (Random.value < absorbChance)
        {
            FloatingTextPool.Instance?.Spawn(
                transform.position + Vector3.up, "ABSORBED", Color.grey);
            return;
        }
        base.TakeDamage(amount);
    }
}