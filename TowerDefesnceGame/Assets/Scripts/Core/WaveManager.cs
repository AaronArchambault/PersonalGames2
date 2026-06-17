using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public string poolTag;       // Must match ObjectPool tag
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

    public List<Wave> waves = new();
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

    public void StartNextWave()
    {
        if (WaveInProgress || currentWaveIndex >= waves.Count) return;
        StartCoroutine(RunWave(waves[currentWaveIndex]));
    }

    IEnumerator RunWave(Wave wave)
    {
        WaveInProgress = true;
        currentWaveIndex++;
        GameManager.Instance.SetWave(currentWaveIndex);
        OnWaveStart?.Invoke(currentWaveIndex);

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
                yield return new WaitForSeconds(info.spawnInterval);
            }
        }

        // Wait for all enemies to die or leak
        yield return new WaitUntil(() => activeEnemyCount <= 0);
        WaveInProgress = false;
        OnWaveComplete?.Invoke();

        if (currentWaveIndex >= waves.Count)
            OnAllWavesComplete?.Invoke();
    }

    void HandleEnemyDied()
    {
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
    }
}