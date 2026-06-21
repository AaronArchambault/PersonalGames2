using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OwlBoss : Enemy
{
    [Header("Owl Boss")]
    public float silenceDuration = 4f;
    public int   silenceCount    = 3;    // silences this many towers
    public float silenceCooldown = 8f;
    public string hootiEffectTag = "Explosion";

    private float silenceTimer = 0f;

    protected override void Update()
    {
        base.Update();
        silenceTimer += Time.deltaTime;
        if (silenceTimer >= silenceCooldown)
        {
            silenceTimer = 0f;
            StartCoroutine(SilenceNearbyTowers());
        }
    }

    IEnumerator SilenceNearbyTowers()
    {
        ObjectPool.Instance.Spawn(hootiEffectTag, transform.position, Quaternion.identity);
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up, "HOOT!", new Color(0.8f, 0.6f, 0.2f));
        AudioManager.Instance?.Play("owl_hoot");

        // Find nearest towers
        var allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        var nearest = allTowers
            .OrderBy(t => Vector2.Distance(transform.position, t.transform.position))
            .Take(silenceCount)
            .ToList();

        // Silence them
        foreach (var tower in nearest)
        {
            if (tower == null) continue;
            tower.enabled = false;
            var sr = tower.GetComponent<SpriteRenderer>();
            if (sr) sr.color = new Color(0.5f, 0.3f, 0.8f); // purple = silenced
            FloatingTextPool.Instance?.Spawn(
                tower.transform.position + Vector3.up, "SILENCED!", Color.magenta);
        }

        yield return new WaitForSeconds(silenceDuration);

        // Re-enable towers
        foreach (var tower in nearest)
        {
            if (tower == null) continue;
            tower.enabled = true;
            var sr = tower.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }
    }

    protected override void Die()
    {
        GameManager.Instance.EarnGold(75);
        UIManager.Instance?.Announce("OWL BOSS DEFEATED!", Color.green);
        base.Die();
    }
}