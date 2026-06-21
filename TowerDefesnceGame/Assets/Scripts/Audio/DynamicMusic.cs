using UnityEngine;
using UnityEngine.Audio;
 
public class DynamicMusic : MonoBehaviour
{
    public static DynamicMusic Instance { get; private set; }
 
    [Header("Audio Sources")]
    public AudioSource calmLayer;
    public AudioSource tensionLayer;
    public AudioSource bossLayer;
 
    [Header("Settings")]
    public float fadeSpeed    = 1.5f;
    public int   enemiesForMaxTension = 12;
 
    [Header("Snapshots")]
    public AudioMixerSnapshot normalSnapshot;
    public AudioMixerSnapshot pausedSnapshot;
 
    private bool isPaused = false;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
 
    void OnEnable()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart    += OnWaveStart;
            WaveManager.Instance.OnWaveComplete += OnWaveComplete;
        }
    }
 
    void OnDisable()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart    -= OnWaveStart;
            WaveManager.Instance.OnWaveComplete -= OnWaveComplete;
        }
    }
 
    void Update()
    {
        if (isPaused) return;
 
        int enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        float tension  = Mathf.Clamp01((float)enemyCount / enemiesForMaxTension);
 
        if (tensionLayer)
            tensionLayer.volume = Mathf.Lerp(tensionLayer.volume, tension, Time.deltaTime * fadeSpeed);
        if (calmLayer)
            calmLayer.volume    = Mathf.Lerp(calmLayer.volume, Mathf.Lerp(0.8f, 0.2f, tension),
                                             Time.deltaTime * fadeSpeed);
    }
 
    void OnWaveStart(int wave)
    {
        // Snap tension up immediately at wave start
        if (tensionLayer) tensionLayer.volume = 0.4f;
    }
 
    void OnWaveComplete()
    {
        AudioManager.Instance?.Play("crowd_cheer");
        // Fade tension out
        if (tensionLayer) tensionLayer.volume = 0f;
    }
 
    public void SetBossMusic(bool active)
    {
        if (bossLayer) bossLayer.volume = active ? 1f : 0f;
        if (calmLayer)    calmLayer.volume    = active ? 0f : 0.8f;
        if (tensionLayer) tensionLayer.volume = active ? 0f : 0f;
    }
 
    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (pausedSnapshot != null && normalSnapshot != null)
        {
            if (paused) pausedSnapshot.TransitionTo(0.1f);
            else        normalSnapshot.TransitionTo(0.1f);
        }
    }
}
 