
using System.Collections.Generic;
using System.IO;
using UnityEngine;
 
[System.Serializable] public class WaveFileData   { public List<WaveFileEntry> waves = new(); }
[System.Serializable] public class WaveFileEntry  { public string waveName; public float prewaveDelay = 1f; public List<EnemyFileEntry> enemies = new(); }
[System.Serializable] public class EnemyFileEntry { public string poolTag; public int count; public float spawnInterval; }
[System.Serializable] public class SavedLevelData { public List<TileEntry> tiles = new(); }
[System.Serializable] public class TileEntry      { public int x, y; public bool isPath; }
 
public class WaveSaveSystem : MonoBehaviour
{
    public static WaveSaveSystem Instance { get; private set; }
 
    // Default file — overridden by LevelSelectManager via SetWaveFile()
    private string waveFileName = "Levels/LivingRoom_1.json";
 
    // The full path used by both LoadWaves and SaveWaves
    string WavePath => Path.Combine(Application.streamingAssetsPath, waveFileName);
 
    public void SetWaveFile(string relativePath)
    {
        waveFileName = relativePath;
        Debug.Log($"[WaveSaveSystem] Wave file set to: {waveFileName}");
    }
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
 
    public List<Wave> LoadWaves()
    {
        if (!File.Exists(WavePath))
        {
            Debug.LogWarning($"[WaveSaveSystem] File not found: {WavePath}");
            return null;
        }
 
        var data = JsonUtility.FromJson<WaveFileData>(File.ReadAllText(WavePath));
        if (data == null || data.waves == null)
        {
            Debug.LogWarning($"[WaveSaveSystem] Failed to parse JSON at: {WavePath}");
            return null;
        }
 
        var result = new List<Wave>();
        foreach (var wd in data.waves)
        {
            var w = new Wave { waveName = wd.waveName, prewaveDelay = wd.prewaveDelay };
            foreach (var e in wd.enemies)
                w.enemies.Add(new EnemySpawnInfo
                {
                    poolTag       = e.poolTag,
                    count         = e.count,
                    spawnInterval = e.spawnInterval
                });
            result.Add(w);
        }
        Debug.Log($"[WaveSaveSystem] Loaded {result.Count} waves from {waveFileName}");
        return result;
    }
 
    public void SaveWaves(List<Wave> waves)
    {
        var data = new WaveFileData();
        foreach (var w in waves)
        {
            var wd = new WaveFileEntry
            {
                waveName     = w.waveName,
                prewaveDelay = w.prewaveDelay
            };
            foreach (var e in w.enemies)
                wd.enemies.Add(new EnemyFileEntry
                {
                    poolTag       = e.poolTag,
                    count         = e.count,
                    spawnInterval = e.spawnInterval
                });
            data.waves.Add(wd);
        }
 
        string dir = Path.GetDirectoryName(WavePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
 
        File.WriteAllText(WavePath, JsonUtility.ToJson(data, true));
        Debug.Log($"[WaveSaveSystem] Waves saved to {WavePath}");
    }
}