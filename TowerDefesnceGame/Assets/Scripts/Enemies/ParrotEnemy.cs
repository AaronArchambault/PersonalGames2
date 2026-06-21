using UnityEngine;
using System.Collections;

public class ParrotEnemy : Enemy
{
    [Header("Parrot")]
    public float mimicCooldown  = 5f;
    public float mimicDuration  = 3f;

    private string lastHitType  = "";  // tracks what type of tower hit it
    private float  mimicTimer   = 0f;
    private bool   isMimicking  = false;

    public override void TakeDamage(float amount)
    {
        // Record what hit it based on damage amount ranges
        // (simplified — in production you'd pass a damage source tag)
        if (amount > 100f)  lastHitType = "sniper";
        else if (amount < 10f) lastHitType = "laser";
        else lastHitType = "basic";

        base.TakeDamage(amount);
    }

    protected override void Update()
    {
        base.Update();
        mimicTimer += Time.deltaTime;
        if (!isMimicking && mimicTimer >= mimicCooldown && lastHitType != "")
        {
            mimicTimer = 0f;
            StartCoroutine(MimicAbility());
        }
    }

    IEnumerator MimicAbility()
    {
        isMimicking = true;
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up, $"COPYING: {lastHitType.ToUpper()}!", Color.cyan);

        switch (lastHitType)
        {
            case "sniper":
                // High speed burst
                float origSpeed = moveSpeed;
                moveSpeed *= 3f;
                yield return new WaitForSeconds(mimicDuration);
                moveSpeed = origSpeed;
                break;

            case "laser":
                // Become immune to damage briefly
                var origTag = gameObject.layer;
                gameObject.layer = LayerMask.NameToLayer("Default");
                FloatingTextPool.Instance?.Spawn(
                    transform.position + Vector3.up, "IMMUNE!", Color.yellow);
                yield return new WaitForSeconds(mimicDuration);
                gameObject.layer = origTag;
                break;

            case "basic":
                // Heal a bit
                currentHealth = Mathf.Min(maxHealth, currentHealth + maxHealth * 0.2f);
                healthBar?.SetFill(currentHealth / maxHealth);
                FloatingTextPool.Instance?.Spawn(
                    transform.position + Vector3.up, "HEALED!", Color.green);
                break;
        }

        isMimicking = false;
    }
}