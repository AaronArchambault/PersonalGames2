using UnityEngine;

public class ArmouredRat : Enemy
{
    [Header("Armour")]
    [Range(0f, 0.9f)]
    public float armourReduction = 0.5f;
    public int   armourHP        = 50;   // armour absorbs this much before breaking
    public SpriteRenderer shieldSprite;

    private int currentArmourHP;
    private bool armourBroken = false;

    public override void OnSpawn()
    {
        base.OnSpawn();
        currentArmourHP = armourHP;
        armourBroken = false;
        if (shieldSprite) shieldSprite.enabled = true;
    }

    public override void TakeDamage(float amount)
    {
        if (!armourBroken)
        {
            float reduced = amount * (1f - armourReduction);
            currentArmourHP -= Mathf.RoundToInt(amount * armourReduction);

            if (currentArmourHP <= 0)
            {
                armourBroken = true;
                if (shieldSprite) shieldSprite.enabled = false;
                FloatingTextPool.Instance?.Spawn(
                    transform.position + Vector3.up, "ARMOUR BROKEN!", Color.grey);
            }
            base.TakeDamage(reduced);
        }
        else
        {
            base.TakeDamage(amount);
        }
    }
}