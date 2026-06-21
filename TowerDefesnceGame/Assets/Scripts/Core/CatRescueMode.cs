using UnityEngine;
using TMPro;

public class CatRescueMode : MonoBehaviour
{
    public static CatRescueMode Instance { get; private set; }

    public int kittensToSave    = 10;   // total kittens in the level
    public int kittensEscaped   = 0;
    public int kittensSaved     = 0;

    public TextMeshProUGUI kittenText;
    public GameObject      kittenCounterPanel;
    public string          kittenCarrierTag = "KittenCarrier";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (GameModeManager.Instance?.IsCatRescue == true)
        {
            if (kittenCounterPanel) kittenCounterPanel.SetActive(true);
            UpdateDisplay();
        }
    }

    public void KittenEscaped()
    {
        kittensEscaped++;
        GameManager.Instance.LoseLife(2); // losing a kitten costs 2 lives
        FloatingTextPool.Instance?.Spawn(
            Vector3.zero, "KITTEN ESCAPED!", Color.red);
        UpdateDisplay();
    }

    public void KittenRescued()
    {
        kittensSaved++;
        GameManager.Instance.EarnGold(30);
        FloatingTextPool.Instance?.Spawn(
            Vector3.zero, "KITTEN SAVED! +30g", Color.yellow);
        UpdateDisplay();

        if (kittensSaved >= kittensToSave)
            UIManager.Instance?.Announce("ALL KITTENS SAVED!", Color.green);
    }

    void UpdateDisplay()
    {
        if (kittenText)
            kittenText.text = $"🐱 {kittensSaved}/{kittensToSave} saved";
    }
}

// Add this component to enemy prefabs in Cat Rescue mode
public class KittenCarrier : MonoBehaviour
{
    public GameObject kittenVisual; // small kitten sprite on the enemy
    private bool      hasKitten = true;

    void OnEnable()
    {
        hasKitten = true;
        if (kittenVisual) kittenVisual.SetActive(true);
    }

    // Called when enemy dies — kitten is rescued
    void OnDisable()
    {
        if (!hasKitten) return;
        CatRescueMode.Instance?.KittenRescued();
    }

    // Called if enemy reaches the end with kitten
    public void EscapeWithKitten()
    {
        hasKitten = false;
        if (kittenVisual) kittenVisual.SetActive(false);
        CatRescueMode.Instance?.KittenEscaped();
    }
}