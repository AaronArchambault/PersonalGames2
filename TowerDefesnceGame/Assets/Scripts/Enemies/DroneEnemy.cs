using UnityEngine;
using System.Collections;

public class DroneEnemy : FlyingEnemy
{
    [Header("Drone Bombs")]
    public float bombCooldown   = 4f;
    public float bombRadius     = 2f;
    public float bombStunTime   = 1.5f;  // stuns towers briefly
    public string bombEffectTag = "Explosion";

    private float bombTimer = 0f;

    protected override void Update()
    {
        base.Update();
        bombTimer += Time.deltaTime;
        if (bombTimer >= bombCooldown)
        {
            bombTimer = 0f;
            DropBomb();
        }
    }

    void DropBomb()
    {
        ObjectPool.Instance.Spawn(bombEffectTag, transform.position, Quaternion.identity);
        CameraShake.Instance?.Shake(0.15f, 0.2f);
        FloatingTextPool.Instance?.Spawn(
            transform.position, "BOMB!", Color.red);

        // Stun towers below
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, bombRadius, LayerMask.GetMask("Tower"));
        foreach (var hit in hits)
            StartCoroutine(StunTower(hit.GetComponent<Tower>()));
    }

    IEnumerator StunTower(Tower t)
    {
        if (t == null) yield break;
        t.enabled = false;
        var sr = t.GetComponent<SpriteRenderer>();
        if (sr) sr.color = Color.yellow;
        FloatingTextPool.Instance?.Spawn(
            t.transform.position + Vector3.up, "STUNNED!", Color.yellow);
        yield return new WaitForSeconds(bombStunTime);
        if (t != null)
        {
            t.enabled = true;
            if (sr) sr.color = Color.white;
        }
    }
}