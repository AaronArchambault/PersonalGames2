using UnityEngine;
using System.Collections;

public class RobotRatEnemy : Enemy
{
    [Header("EMP")]
    public float empRadius    = 3f;
    public float empDuration  = 2f;
    public float empCooldown  = 8f;
    public string empEffectTag = "Explosion";

    private float empTimer = 0f;

    protected override void Update()
    {
        base.Update();
        empTimer += Time.deltaTime;
        if (empTimer >= empCooldown)
        {
            empTimer = 0f;
            StartCoroutine(FireEMP());
        }
    }

    IEnumerator FireEMP()
    {
        ObjectPool.Instance.Spawn(empEffectTag, transform.position, Quaternion.identity);
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up, "EMP!", new Color(0.4f, 1f, 1f));

        // Disable towers nearby temporarily
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, empRadius, LayerMask.GetMask("Tower"));

        var disabledTowers = new System.Collections.Generic.List<Tower>();
        foreach (var hit in hits)
        {
            var t = hit.GetComponent<Tower>();
            if (t == null) continue;
            t.enabled = false;
            disabledTowers.Add(t);

            // Visual: turn grey
            var sr = hit.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.grey;
        }

        yield return new WaitForSeconds(empDuration);

        // Re-enable towers
        foreach (var t in disabledTowers)
        {
            if (t == null) continue;
            t.enabled = true;
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }
    }
}