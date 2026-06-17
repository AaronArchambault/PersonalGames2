using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// GameManager — singleton. Tracks score, tier unlocks, and game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Data class (no [Header] inside — that caused the CS0592 error) ──
    [System.Serializable]
    public class TierUnlock
    {
        public string tierName   = "Fish";
        public float  massNeeded = 10f;
        [HideInInspector] public bool triggered = false;
    }

    // ── Inspector ──────────────────────────────────────────
    [Header("Tier Unlock Thresholds")]
    public List<TierUnlock> tierUnlocks = new List<TierUnlock>
    {
        new TierUnlock { tierName = "Minnows",     massNeeded = 0    },
        new TierUnlock { tierName = "Small Fish",  massNeeded = 10   },
        new TierUnlock { tierName = "Big Fish",    massNeeded = 30   },
        new TierUnlock { tierName = "Humans",      massNeeded = 70   },
        new TierUnlock { tierName = "Boats",       massNeeded = 150  },
        new TierUnlock { tierName = "Buildings",   massNeeded = 400  },
        new TierUnlock { tierName = "Skyscrapers", massNeeded = 900  },
        new TierUnlock { tierName = "Cities",      massNeeded = 2000 },
    };

    [Header("Events")]
    public UnityEvent<float, float> onMassChanged;      // (massEaten, totalMass)
    public UnityEvent<float, float> onPlayerDamaged;    // (massDamage, totalMass)
    public UnityEvent<string>       onTierUnlocked;     // (tierName)
    public UnityEvent               onGameOver;
    public UnityEvent               onVictory;

    // ── Runtime ────────────────────────────────────────────
    public float TotalMass  { get; private set; }
    public bool  IsGameOver { get; private set; }

    private WorldSpawner spawner;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        spawner = Object.FindAnyObjectByType<WorldSpawner>();
        tierUnlocks.Sort((a, b) => a.massNeeded.CompareTo(b.massNeeded));
    }

    // ── Called by PlayerCreature ───────────────────────────
    public void OnPlayerAte(float massEaten, float totalMass)
    {
        TotalMass = totalMass;
        onMassChanged?.Invoke(massEaten, TotalMass);
        CheckTierUnlocks(TotalMass);
    }

    public void OnPlayerDamaged(float massDamage, float totalMass)
    {
        TotalMass = totalMass;
        onPlayerDamaged?.Invoke(massDamage, TotalMass);
    }

    void CheckTierUnlocks(float mass)
    {
        foreach (var tier in tierUnlocks)
        {
            if (!tier.triggered && mass >= tier.massNeeded)
            {
                tier.triggered = true;
                onTierUnlocked?.Invoke(tier.tierName);
                spawner?.ForceSpawnTier(tier.tierName);
                Debug.Log($"[GameManager] Tier unlocked: {tier.tierName}");
            }
        }
    }

    // ── Game Flow ──────────────────────────────────────────
    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        onGameOver?.Invoke();
        Time.timeScale = 0f;
    }

    public void TriggerVictory()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        onVictory?.Invoke();
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}