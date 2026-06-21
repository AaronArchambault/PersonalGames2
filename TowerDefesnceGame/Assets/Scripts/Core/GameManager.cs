
using UnityEngine;
using UnityEngine.SceneManagement;
 
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
 
    [Header("Player Stats")]
    public int startingLives = 20;
    public int startingGold  = 150;
 
    public int Lives { get; private set; }
    public int Gold  { get; private set; }
    public int Wave  { get; private set; }
    public bool GameOver { get; private set; }
 
    // Events
    public event System.Action<int> OnLivesChanged;
    public event System.Action<int> OnGoldChanged;
    public event System.Action<int> OnWaveChanged;
    public event System.Action      OnGameOver;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }
 
    void Start()
    {
        Lives    = startingLives;
        Gold     = startingGold;
        Wave     = 0;
        GameOver = false;
    }
 
    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
 
    public void EarnGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }
 
    public void LoseLife(int amount = 1)
    {
        Lives = Mathf.Max(0, Lives - amount);
        OnLivesChanged?.Invoke(Lives);
        CameraShake.Instance?.Shake(0.15f, 0.3f);
        if (Lives <= 0) TriggerGameOver();
    }
 
    // Used by PowerUpManager and anything else that needs to add lives
    // from outside this class — events can only be invoked from inside
    public void AddLives(int amount)
    {
        Lives += amount;
        OnLivesChanged?.Invoke(Lives);
    }
 
    public void SetWave(int w)
    {
        Wave = w;
        OnWaveChanged?.Invoke(Wave);
    }
 
    void TriggerGameOver()
    {
        GameOver = true;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
    }
 
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}