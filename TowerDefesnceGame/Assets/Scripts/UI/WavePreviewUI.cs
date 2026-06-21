
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
 
public class WavePreviewUI : MonoBehaviour
{
    public static WavePreviewUI Instance { get; private set; }
 
    [Header("UI")]
    public GameObject   previewPanel;
    public Transform    iconContainer;   // Horizontal Layout Group
    public GameObject   enemyIconPrefab; // Image + TextMeshProUGUI child
 
    [Header("Enemy Icons")]
    public List<EnemyIconEntry> enemyIcons = new();
 
    [System.Serializable]
    public class EnemyIconEntry
    {
        public string poolTag;
        public Sprite icon;
        public Color  color = Color.white;
    }
 
    private Dictionary<string, EnemyIconEntry> iconLookup = new();
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        foreach (var e in enemyIcons) iconLookup[e.poolTag] = e;
    }
 
    void OnEnable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete += ShowNextWavePreview;
    }
 
    void OnDisable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= ShowNextWavePreview;
    }
 
    void ShowNextWavePreview()
    {
        if (WaveManager.Instance == null || previewPanel == null) return;
 
        var waves = WaveManager.Instance.waves;
        int next  = WaveManager.Instance.CurrentWaveIndex; // index of next wave
        if (next >= waves.Count) { previewPanel.SetActive(false); return; }
 
        // Clear old icons
        foreach (Transform child in iconContainer) Destroy(child.gameObject);
 
        // Count each enemy type
        var counts = new Dictionary<string, int>();
        foreach (var info in waves[next].enemies)
        {
            if (!counts.ContainsKey(info.poolTag)) counts[info.poolTag] = 0;
            counts[info.poolTag] += info.count;
        }
 
        // Create icon per type
        foreach (var kvp in counts)
        {
            var obj   = Instantiate(enemyIconPrefab, iconContainer);
            var img   = obj.GetComponentInChildren<Image>();
            var label = obj.GetComponentInChildren<TextMeshProUGUI>();
 
            if (iconLookup.TryGetValue(kvp.Key, out var entry))
            {
                if (img)   { img.sprite = entry.icon; img.color = entry.color; }
            }
            if (label) label.text = $"x{kvp.Value}";
        }
 
        previewPanel.SetActive(true);
    }
}