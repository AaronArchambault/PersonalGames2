using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RatKing : Enemy
{
    [Header("Rat King")]
    public string orbitRatTag    = "ScoutMouse";
    public int    orbitRatCount  = 4;
    public float  orbitRadius    = 1.2f;
    public float  orbitSpeed     = 90f;     // degrees per second
    public float  shieldDamageReduction = 0.8f; // 80% reduction while rats alive

    [Header("Rage Phase")]
    public float rageSpeedMultiplier = 1.5f;
    public Color rageColor           = Color.red;

    private List<GameObject> orbitRats = new();
    private bool rageActive = false;
    private float orbitAngle = 0f;

    public override void OnSpawn()
    {
        base.OnSpawn();
        orbitRats.Clear();
        rageActive = false;
        StartCoroutine(SpawnOrbitRats());
        UIManager.Instance?.Announce("THE RAT KING APPROACHES!", Color.red);
    }

    IEnumerator SpawnOrbitRats()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < orbitRatCount; i++)
        {
            float angle = i * (360f / orbitRatCount);
            Vector3 pos = transform.position +
                new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad),
                            Mathf.Sin(angle * Mathf.Deg2Rad), 0) * orbitRadius;
            var rat = ObjectPool.Instance.Spawn(orbitRatTag, pos, Quaternion.identity);
            if (rat != null) orbitRats.Add(rat);
        }
    }

    protected override void Update()
    {
        base.Update();
        UpdateOrbitRats();
        CheckRagePhase();
    }

    void UpdateOrbitRats()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        // Remove dead rats
        orbitRats.RemoveAll(r => r == null || !r.activeInHierarchy);

        for (int i = 0; i < orbitRats.Count; i++)
        {
            if (orbitRats[i] == null) continue;
            float angle = orbitAngle + i * (360f / orbitRatCount);
            orbitRats[i].transform.position = transform.position +
                new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad),
                            Mathf.Sin(angle * Mathf.Deg2Rad), 0) * orbitRadius;
        }
    }

    void CheckRagePhase()
    {
        // When all rats dead and HP below 50%, go rage
        if (!rageActive && orbitRats.Count == 0 && currentHealth < maxHealth * 0.5f)
        {
            rageActive = true;
            moveSpeed *= rageSpeedMultiplier;
            if (sr) sr.color = rageColor;
            UIManager.Instance?.Announce("RAT KING ENRAGED!", Color.red);
            CameraShake.Instance?.Shake(0.3f, 0.5f);
        }
    }

    public override void TakeDamage(float amount)
    {
        // Rats absorb most damage while alive
        if (orbitRats.Count > 0)
        {
            amount *= (1f - shieldDamageReduction);
            FloatingTextPool.Instance?.Spawn(
                transform.position + Vector3.up, "BLOCKED!", Color.grey);
        }
        base.TakeDamage(amount);
    }

    protected override void Die()
    {
        // Kill remaining orbit rats
        foreach (var rat in orbitRats)
            if (rat != null) rat.GetComponent<Enemy>()?.TakeDamage(9999f);

        GameManager.Instance.EarnGold(100); // big boss reward
        UIManager.Instance?.Announce("RAT KING DEFEATED!", Color.green);
        CameraShake.Instance?.Shake(0.4f, 0.6f);
        base.Die();
    }
}