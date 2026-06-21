using UnityEngine;
using TMPro;

public class SpeedRunTimer : MonoBehaviour
{
    public static SpeedRunTimer Instance { get; private set; }

    public TextMeshProUGUI timerText;
    public bool isRunning = false;

    private float elapsed = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartTimer()
    {
        elapsed   = 0f;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
        SaveTime();
    }

    void Update()
    {
        if (!isRunning) return;
        elapsed += Time.deltaTime;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (timerText == null) return;
        int minutes = (int)(elapsed / 60f);
        int seconds = (int)(elapsed % 60f);
        int ms      = (int)((elapsed * 100f) % 100f);
        timerText.text = $"{minutes:00}:{seconds:00}.{ms:00}";

        // Turn red when going slow
        timerText.color = elapsed > 300f ? Color.red
                        : elapsed > 180f ? Color.yellow
                        : Color.white;
    }

    void SaveTime()
    {
        float best = PlayerPrefs.GetFloat("speedrun_best", float.MaxValue);
        if (elapsed < best)
        {
            PlayerPrefs.SetFloat("speedrun_best", elapsed);
            UIManager.Instance?.Announce("NEW BEST TIME!", Color.yellow);
        }
        int minutes = (int)(elapsed / 60f);
        int seconds = (int)(elapsed % 60f);
        UIManager.Instance?.Announce($"TIME: {minutes:00}:{seconds:00}", Color.white);
    }

    public string GetBestTimeString()
    {
        float best = PlayerPrefs.GetFloat("speedrun_best", 0f);
        if (best == 0f) return "No record";
        int m = (int)(best / 60f);
        int s = (int)(best % 60f);
        return $"{m:00}:{s:00}";
    }
}