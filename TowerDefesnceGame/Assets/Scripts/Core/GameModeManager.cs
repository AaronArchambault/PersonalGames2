using UnityEngine;

public enum GameMode
{
    Normal,
    Endless,
    SpeedRun,
    CatRescue,
    Sandbox,
    DailyChallenge
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public GameMode currentMode = GameMode.Normal;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void SetMode(GameMode mode)
    {
        currentMode = mode;
        ApplyModeRules(mode);
    }

    void ApplyModeRules(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Sandbox:
                // Unlimited gold — set a huge amount
                while (GameManager.Instance.Gold < 999999)
                    GameManager.Instance.EarnGold(999999);
                WaveManager.Instance.enabled = false;
                break;

            case GameMode.SpeedRun:
                SpeedRunTimer.Instance?.StartTimer();
                break;
        }
    }

    public bool IsSandbox      => currentMode == GameMode.Sandbox;
    public bool IsEndless      => currentMode == GameMode.Endless;
    public bool IsSpeedRun     => currentMode == GameMode.SpeedRun;
    public bool IsCatRescue    => currentMode == GameMode.CatRescue;
    public bool IsDailyChallenge => currentMode == GameMode.DailyChallenge;
}