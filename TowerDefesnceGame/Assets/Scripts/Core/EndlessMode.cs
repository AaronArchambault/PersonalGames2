using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessMode : MonoBehaviour
{
    public static EndlessMode Instance { get; private set; }

    [Header("Endless Scaling")]
    public float healthScalePerWave  = 1.15f;  // enemies get 15% more HP each wave
    public float speedScalePerWave   = 1.05f;  // 5% faster each wave
    public int   goldScalePerWave    = 2;      // extra gold reward per wave number
    public float spawnRateScale      = 0.95f;  // enemies spawn 5% faster each wave

    [Header("Enemy Pool")]
    public List<string> enemyPoolTags = new()
    {
        "ScoutMouse", "ArmouredRat", "CrowEnemy",
        "NinjaMouse", "CheeseBomber", "SeagullEnemy",
        "RoombaEnemy", "RobotRatEnemy"
    };

    private int endlessWave = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        if (GameModeManager.Instance?.IsEndless == true)
            WaveManager.Instance.OnWaveComplete += SpawnNextEndlessWave;
    }

    void OnDisable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= SpawnNextEndlessWave;
    }

    void SpawnNextEndlessWave()
    {
        endlessWave++;
        StartCoroutine(GenerateAndStartWave(endlessWave));
    }

    IEnumerator GenerateAndStartWave(int waveNum)
    {
        yield return new WaitForSeconds(3f);

        // Build a wave procedurally
        var wave = new Wave
        {
            waveName = $"Endless Wave {waveNum}",
            prewaveDelay = 1f
        };

        int enemyCount = 5 + waveNum * 2;
        float interval = Mathf.Max(0.2f, 0.8f * Mathf.Pow(spawnRateScale, waveNum));

        // Mix of enemy types, getting harder
        for (int i = 0; i < enemyCount; i++)
        {
            string tag = enemyPoolTags[
                Mathf.Min(waveNum / 2, enemyPoolTags.Count - 1)];
            // Random variety
            if (Random.value > 0.6f)
                tag = enemyPoolTags[Random.Range(0, Mathf.Min(waveNum + 1,
                      enemyPoolTags.Count))];

            wave.enemies.Add(new EnemySpawnInfo
            {
                poolTag = tag,
                count = 1,
                spawnInterval = interval
            });
        }

        // Add boss every 5 waves
        if (waveNum % 5 == 0)
        {
            wave.enemies.Add(new EnemySpawnInfo
            {
                poolTag = "MegaRoomba",
                count = 1,
                spawnInterval = 3f
            });
        }

        WaveManager.Instance.waves.Add(wave);
        WaveManager.Instance.StartNextWave();

        UIManager.Instance?.Announce($"ENDLESS WAVE {waveNum}!", Color.orange);
    }
}