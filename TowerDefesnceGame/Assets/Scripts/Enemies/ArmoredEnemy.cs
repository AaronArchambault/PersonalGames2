using UnityEngine;

public class ArmoredEnemy : Enemy
{
    [Range(0f, 0.9f)]
    public float damageReduction = 0.5f;

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount * (1f - damageReduction));
    }
}