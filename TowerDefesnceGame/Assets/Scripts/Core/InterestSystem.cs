using UnityEngine;

public class InterestSystem : MonoBehaviour
{
    public static InterestSystem Instance { get; private set; }

    [Header("Interest")]
    public float interestRate = 0.01f;   // 1% per wave
    public int   maxInterest  = 50;      // cap so early game isn't ruined

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete += PayInterest;
    }

    void OnDisable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= PayInterest;
    }

    void PayInterest()
    {
        int interest = Mathf.Min(
            Mathf.RoundToInt(GameManager.Instance.Gold * interestRate),
            maxInterest);

        if (interest <= 0) return;

        GameManager.Instance.EarnGold(interest);
        FloatingTextPool.Instance?.Spawn(
            Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.7f, 10f)),
            $"+{interest}g INTEREST", Color.yellow);

        UIManager.Instance?.Announce($"Wave interest: +{interest}g", Color.yellow);
    }
}