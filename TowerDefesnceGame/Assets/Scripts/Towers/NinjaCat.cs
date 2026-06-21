using UnityEngine;
using System.Collections;

public class NinjaCat : Tower
{
    [Header("Ninja")]
    public float teleportDamage   = 200f;
    public float teleportCooldown = 3f;
    public float disappearTime    = 0.3f;  // invisible for this long after teleport

    [Header("Target Priority")]
    public float hpThreshold = 0.7f;   // targets enemies above this HP fraction

    [Header("Visuals")]
    public string smokeEffectTag   = "SmokeEffect";
    public SpriteRenderer catSprite;
    public TrailRenderer  shurikenTrail;

    private float teleportTimer = 0f;
    private bool  isVisible     = true;

    protected override void Update()
    {
        teleportTimer += Time.deltaTime;
        if (teleportTimer >= teleportCooldown)
        {
            teleportTimer = 0f;
            TryTeleportAssassinate();
        }
    }

    void TryTeleportAssassinate()
    {
        // Find highest-HP enemy in range
        Enemy bestTarget = null;
        float bestHP = 0f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, Range, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            var e = hit.GetComponent<Enemy>();
            if (e == null) continue;
            if (e.GetHealth() > bestHP)
            {
                bestHP = e.GetHealth();
                bestTarget = e;
            }
        }

        if (bestTarget == null) return;
        StartCoroutine(TeleportStrike(bestTarget));
    }

    IEnumerator TeleportStrike(Enemy target)
    {
        // Disappear
        ObjectPool.Instance.Spawn(smokeEffectTag, transform.position, Quaternion.identity);
        if (catSprite) catSprite.enabled = false;
        isVisible = false;
        GetComponent<TowerAnimator>()?.SetInvisible(true);

        yield return new WaitForSeconds(disappearTime);

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            if (catSprite) catSprite.enabled = true;
            isVisible = true;
            yield break;
        }

        // Deal damage
        target.TakeDamage(teleportDamage + Damage);
        ObjectPool.Instance.Spawn(smokeEffectTag, target.transform.position, Quaternion.identity);
        FloatingTextPool.Instance?.Spawn(
            target.transform.position + Vector3.up, "ASSASSINATE!", Color.cyan);

        // Reappear
        yield return new WaitForSeconds(0.1f);
        if (catSprite) catSprite.enabled = true;
        isVisible = true;
        GetComponent<TowerAnimator>()?.SetInvisible(false);
    }

    protected override void Shoot() { } // Ninja uses teleport system instead
}