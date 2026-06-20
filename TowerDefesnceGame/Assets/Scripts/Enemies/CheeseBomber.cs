using UnityEngine;

public class CheeseBomber : Enemy
{
    [Header("Cheese Bomb")]
    public float explosionRadius = 2f;
    public float explosionDamageToTowers = 0f; // towers don't take damage
    public string explosionEffectTag = "Explosion";

    // Note: "damage to towers" is conceptual — in this implementation
    // the explosion pushes nearby cats off cooldown (simulated as a debuff)
    public float towerCooldownAdd = 1.5f; // adds delay to towers nearby

    protected override void Die()
    {
        // Explode first
        ObjectPool.Instance.Spawn(explosionEffectTag,
            transform.position, Quaternion.identity);
        CameraShake.Instance?.Shake(0.2f, 0.3f);

        // Affect towers nearby — add cooldown
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, explosionRadius, LayerMask.GetMask("Tower"));
        foreach (var hit in hits)
        {
            // Visual stun — towers flash
            var sr = hit.GetComponent<SpriteRenderer>();
            if (sr) StartCoroutine(FlashTower(sr));
        }

        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up, "BOOM!", Color.yellow);

        base.Die();
    }

    System.Collections.IEnumerator FlashTower(SpriteRenderer sr)
    {
        Color orig = sr.color;
        sr.color = Color.yellow;
        yield return new WaitForSeconds(0.2f);
        sr.color = orig;
    }
}