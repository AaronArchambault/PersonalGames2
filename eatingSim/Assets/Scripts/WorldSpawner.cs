using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// WorldSpawner — spawns Edibles, Obstacles, and Enemies around the player.
/// Tiers unlock as the player grows.
/// </summary>
public class WorldSpawner : MonoBehaviour
{
    // ── Data ───────────────────────────────────────────────
    [System.Serializable]
    public class EdibleTier
    {
        public string     tierName      = "Minnow";
        public GameObject prefab;
        public float      unlockAtMass  = 0f;
        public float      massValue     = 1f;
        public float      spawnWeight   = 1f;
    }

    [System.Serializable]
    public class ObstacleTier
    {
        public string     obstacleName  = "Jellyfish";
        public GameObject prefab;
        public float      unlockAtMass  = 0f;
        public float      spawnWeight   = 1f;
    }

    [System.Serializable]
    public class EnemyTier
    {
        public string     enemyName     = "Big Fish";
        public GameObject prefab;
        public float      unlockAtMass  = 5f;
        public float      spawnWeight   = 0.5f;
    }

    // ── Inspector ──────────────────────────────────────────
    [Header("Edible Tiers")]
    public List<EdibleTier>   edibleTiers   = new List<EdibleTier>();

    [Header("Obstacle Tiers")]
    public List<ObstacleTier> obstacleTiers = new List<ObstacleTier>();

    [Header("Enemy Tiers")]
    public List<EnemyTier>    enemyTiers    = new List<EnemyTier>();

    [Header("Population Targets")]
    public int   targetEdibles   = 35;
    public int   targetObstacles = 8;
    public int   targetEnemies   = 5;

    [Header("Radius")]
    public float spawnRadius     = 22f;
    public float despawnRadius   = 28f;
    public float spawnInterval   = 1.5f;

    // ── Runtime ────────────────────────────────────────────
    private Transform      playerTransform;
    private PlayerCreature player;
    private float          timer;
    private List<GameObject> activeEdibles   = new List<GameObject>();
    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<GameObject> activeEnemies   = new List<GameObject>();

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) { playerTransform = p.transform; player = p.GetComponent<PlayerCreature>(); }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = spawnInterval;

        float mass = player != null ? player.Mass : 0f;

        DespawnFar(activeEdibles);
        DespawnFar(activeObstacles);
        DespawnFar(activeEnemies);

        SpawnEdibles(mass);
        SpawnObstacles(mass);
        SpawnEnemies(mass);
    }

    // ── Edibles ────────────────────────────────────────────
    void SpawnEdibles(float mass)
    {
        activeEdibles.RemoveAll(e => e == null);
        int needed = targetEdibles - activeEdibles.Count;
        for (int i = 0; i < needed; i++)
        {
            var tier = PickEdibleTier(mass);
            if (tier == null) break;
            Vector2 pos = RandomSpawnPos();
            var go      = Instantiate(tier.prefab, pos, Quaternion.identity);
            var edible  = go.GetComponent<Edible>();
            if (edible) edible.massValue = tier.massValue;
            activeEdibles.Add(go);
        }
    }

    EdibleTier PickEdibleTier(float mass)
    {
        var unlocked = edibleTiers.FindAll(t => t.prefab != null && t.unlockAtMass <= mass);
        if (unlocked.Count == 0) return null;
        float total = 0f;
        unlocked.ForEach(t => total += t.spawnWeight);
        float roll = Random.Range(0f, total), acc = 0f;
        foreach (var t in unlocked) { acc += t.spawnWeight; if (roll <= acc) return t; }
        return unlocked[unlocked.Count - 1];
    }

    // ── Obstacles ──────────────────────────────────────────
    void SpawnObstacles(float mass)
    {
        activeObstacles.RemoveAll(e => e == null);
        int needed = targetObstacles - activeObstacles.Count;
        for (int i = 0; i < needed; i++)
        {
            var tier = PickObstacleTier(mass);
            if (tier == null) break;
            var go = Instantiate(tier.prefab, RandomSpawnPos(), Quaternion.identity);
            activeObstacles.Add(go);
        }
    }

    ObstacleTier PickObstacleTier(float mass)
    {
        var unlocked = obstacleTiers.FindAll(t => t.prefab != null && t.unlockAtMass <= mass);
        if (unlocked.Count == 0) return null;
        float total = 0f;
        unlocked.ForEach(t => total += t.spawnWeight);
        float roll = Random.Range(0f, total), acc = 0f;
        foreach (var t in unlocked) { acc += t.spawnWeight; if (roll <= acc) return t; }
        return unlocked[unlocked.Count - 1];
    }

    // ── Enemies ────────────────────────────────────────────
    void SpawnEnemies(float mass)
    {
        activeEnemies.RemoveAll(e => e == null);
        int needed = targetEnemies - activeEnemies.Count;
        for (int i = 0; i < needed; i++)
        {
            var tier = PickEnemyTier(mass);
            if (tier == null) break;
            var go = Instantiate(tier.prefab, RandomSpawnPos(), Quaternion.identity);
            activeEnemies.Add(go);
        }
    }

    EnemyTier PickEnemyTier(float mass)
    {
        var unlocked = enemyTiers.FindAll(t => t.prefab != null && t.unlockAtMass <= mass);
        if (unlocked.Count == 0) return null;
        float total = 0f;
        unlocked.ForEach(t => total += t.spawnWeight);
        float roll = Random.Range(0f, total), acc = 0f;
        foreach (var t in unlocked) { acc += t.spawnWeight; if (roll <= acc) return t; }
        return unlocked[unlocked.Count - 1];
    }

    // ── Helpers ────────────────────────────────────────────
    Vector2 RandomSpawnPos()
    {
        if (playerTransform == null) return Random.insideUnitCircle * spawnRadius;
        return (Vector2)playerTransform.position
             + Random.insideUnitCircle.normalized * Random.Range(spawnRadius * 0.5f, spawnRadius);
    }

    void DespawnFar(List<GameObject> list)
    {
        if (playerTransform == null) return;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null) { list.RemoveAt(i); continue; }
            if (Vector2.Distance(list[i].transform.position, playerTransform.position) > despawnRadius)
            {
                Destroy(list[i]);
                list.RemoveAt(i);
            }
        }
    }

    public void ForceSpawnTier(string tierName)
    {
        var tier = edibleTiers.Find(t => t.tierName == tierName);
        if (tier == null || playerTransform == null) return;
        for (int i = 0; i < 5; i++)
        {
            var go = Instantiate(tier.prefab, RandomSpawnPos(), Quaternion.identity);
            activeEdibles.Add(go);
        }
    }
}