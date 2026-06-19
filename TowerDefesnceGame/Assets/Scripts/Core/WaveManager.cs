using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public string poolTag;
    public int count;
    public float spawnInterval;
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<EnemySpawnInfo> enemies = new();
    public float prewaveDelay = 1f;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Source")]
    [Tooltip("ON = load waves from waves.json  |  OFF = use the Waves list below")]
    public bool loadFromJSON = true;

    [Header("Manual Waves (used when loadFromJSON is OFF)")]
    public List<Wave> waves = new();

    [Header("Spawn")]
    public Transform spawnPoint;

    public bool WaveInProgress { get; private set; }

    public event System.Action<int> OnWaveStart;
    public event System.Action      OnWaveComplete;
    public event System.Action      OnAllWavesComplete;

    private int currentWaveIndex = 0;
    private int activeEnemyCount = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (loadFromJSON)
        {
            if (WaveSaveSystem.Instance == null)
            {
                Debug.LogError("[WaveManager] WaveSaveSystem not found! Add it to your Managers GameObject.");
                return;
            }

            var loaded = WaveSaveSystem.Instance.LoadWaves();
            if (loaded != null && loaded.Count > 0)
            {
                waves = loaded;
                Debug.Log($"[WaveManager] Loaded {waves.Count} waves from JSON.");
            }
            else
            {
                Debug.LogWarning("[WaveManager] JSON load failed or file empty — falling back to manual waves list.");
            }
        }
        else
        {
            Debug.Log($"[WaveManager] Using {waves.Count} manually configured waves.");
        }
    }

    public void StartNextWave()
    {
        if (WaveInProgress)
        {
            Debug.Log("[WaveManager] Wave already in progress.");
            return;
        }
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("[WaveManager] All waves complete!");
            OnAllWavesComplete?.Invoke();
            return;
        }
        StartCoroutine(RunWave(waves[currentWaveIndex]));
    }

    IEnumerator RunWave(Wave wave)
    {
        WaveInProgress = true;
        currentWaveIndex++;
        GameManager.Instance.SetWave(currentWaveIndex);
        OnWaveStart?.Invoke(currentWaveIndex);

        Debug.Log($"[WaveManager] Starting: {wave.waveName}");

        yield return new WaitForSeconds(wave.prewaveDelay);

        foreach (var info in wave.enemies)
        {
            for (int i = 0; i < info.count; i++)
            {
                var obj = ObjectPool.Instance.Spawn(info.poolTag, spawnPoint.position, Quaternion.identity);
                if (obj != null)
                {
                    activeEnemyCount++;
                    var enemy = obj.GetComponent<Enemy>();
                    if (enemy != null) enemy.OnDied += HandleEnemyDied;
                }
                else
                {
                    Debug.LogWarning($"[WaveManager] Could not spawn '{info.poolTag}' — is the prefab assigned in ObjectPool?");
                }
                yield return new WaitForSeconds(info.spawnInterval);
            }
        }

        // Wait until all enemies are gone
        yield return new WaitUntil(() => activeEnemyCount <= 0);

        WaveInProgress = false;
        OnWaveComplete?.Invoke();
        Debug.Log($"[WaveManager] Wave {currentWaveIndex} complete!");

        if (currentWaveIndex >= waves.Count)
            OnAllWavesComplete?.Invoke();
    }

    void HandleEnemyDied()
    {
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
    }
}