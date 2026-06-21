using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LevelThemeManager : MonoBehaviour
{
    public static LevelThemeManager Instance { get; private set; }

    [Header("Active Theme")]
    public LevelTheme activeTheme;

    [Header("References")]
    public SpriteRenderer backgroundRenderer;
    public Light2D        globalLight;          // 2D global light
    public GameObject     windIndicatorUI;
    public GameObject     basementLampSystem;
    public GameObject     tvBeacon;
    public GameObject     fridgeFortification;
    public GameObject     birdbathObjects;

    // Static accessors used by other scripts
    public static float TowerCostMult     => Instance != null ? Instance.activeTheme.towerCostMultiplier     : 1f;
    public static float TowerDamageMult   => Instance != null ? Instance.activeTheme.towerDamageMultiplier   : 1f;
    public static float TowerRangeMult    => Instance != null ? Instance.activeTheme.towerRangeMultiplier    : 1f;
    public static float TowerFireRateMult => Instance != null ? Instance.activeTheme.towerFireRateMultiplier : 1f;
    public static float EnemySpeedMult    => Instance != null ? Instance.activeTheme.enemySpeedMultiplier    : 1f;
    public static float EnemyHealthMult   => Instance != null ? Instance.activeTheme.enemyHealthMultiplier   : 1f;
    public static Vector2 WindForce       => Instance != null ? Instance.activeTheme.windForce               : Vector2.zero;
    public static bool HasWind            => Instance != null && Instance.activeTheme.hasWindEffect;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (activeTheme != null)
            ApplyTheme(activeTheme);
    }

    public void ApplyTheme(LevelTheme theme)
    {
        activeTheme = theme;

        // Background
        if (backgroundRenderer && theme.backgroundSprite)
            backgroundRenderer.sprite = theme.backgroundSprite;

        // Lighting
        if (globalLight)
        {
            globalLight.color     = theme.ambientLightColor;
            globalLight.intensity = theme.ambientLightIntensity;
        }

        // Special objects
        if (tvBeacon)          tvBeacon.SetActive(theme.hasTVBeacon);
        if (fridgeFortification) fridgeFortification.SetActive(theme.hasFridgeBonus);
        if (birdbathObjects)   birdbathObjects.SetActive(theme.hasBirdbathAttraction);
        if (basementLampSystem) basementLampSystem.SetActive(theme.hasLampSystem);
        if (windIndicatorUI)   windIndicatorUI.SetActive(theme.hasWindEffect);

        // Announce theme
        UIManager.Instance?.Announce($"{theme.themeName}", theme.themeColor);
        UIManager.Instance?.ShowThemeDescription(theme.themeDescription);

        // Start special systems
        if (theme.hasWindEffect)   StartCoroutine(WindVarianceLoop());
        if (theme.hasLampSystem)   StartCoroutine(LampRangeSystem());
        if (theme.hasBirdbathAttraction) StartCoroutine(BirdbathAttraction());
        if (theme.hasTVBeacon)     StartCoroutine(TVBeaconPulse());
    }

    // ── ROOFTOP — Wind ────────────────────────────────

    IEnumerator WindVarianceLoop()
    {
        while (activeTheme.hasWindEffect)
        {
            // Wind shifts every few seconds
            yield return new WaitForSeconds(Random.Range(3f, 7f));
            float newX = Random.Range(-2f, 2f);
            float newY = Random.Range(-0.5f, 0.5f);
            activeTheme.windForce = new Vector2(newX, newY);

            // Show wind direction on UI
            UIManager.Instance?.ShowWindIndicator(activeTheme.windForce);
        }
    }

    // ── BASEMENT — Lamp Range System ──────────────────

    IEnumerator LampRangeSystem()
    {
        while (activeTheme.hasLampSystem)
        {
            yield return new WaitForSeconds(0.5f);
            // Towers near lamps get full range; others get reduced range
            // Lamps are tagged "Lamp" in the scene
            var lamps = GameObject.FindGameObjectsWithTag("Lamp");
            var towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);

            foreach (var tower in towers)
            {
                bool nearLamp = false;
                foreach (var lamp in lamps)
                {
                    if (Vector2.Distance(tower.transform.position, lamp.transform.position) < 2.5f)
                    {
                        nearLamp = true; break;
                    }
                }
                // Store near-lamp state on tower
                var lampStatus = tower.GetComponent<BasementLampStatus>();
                if (lampStatus == null)
                    lampStatus = tower.gameObject.AddComponent<BasementLampStatus>();
                lampStatus.nearLamp = nearLamp;
            }
        }
    }

    // ── GARDEN — Birdbath Attracts Birds ──────────────

    IEnumerator BirdbathAttraction()
    {
        while (activeTheme.hasBirdbathAttraction)
        {
            yield return new WaitForSeconds(15f);
            // Spawn extra bird enemies near birdbaths
            var birdbaths = GameObject.FindGameObjectsWithTag("Birdbath");
            if (birdbaths.Length == 0) continue;

            var bath = birdbaths[Random.Range(0, birdbaths.Length)];
            var obj = ObjectPool.Instance.Spawn("PigeonHorde",
                bath.transform.position, Quaternion.identity);

            if (obj != null)
                FloatingTextPool.Instance?.Spawn(
                    bath.transform.position + Vector3.up, "Birds attracted!", Color.cyan);
        }
    }

    // ── LIVING ROOM — TV Beacon ───────────────────────

    IEnumerator TVBeaconPulse()
    {
        while (activeTheme.hasTVBeacon)
        {
            yield return new WaitForSeconds(20f);
            // TV flickers and briefly doubles enemy speed (they rush toward it)
            UIManager.Instance?.Announce("TV TURNED ON — ENEMIES RUSH!", Color.yellow);

            float origSpeed = activeTheme.enemySpeedMultiplier;
            activeTheme.enemySpeedMultiplier = origSpeed * 2f;
            yield return new WaitForSeconds(5f);
            activeTheme.enemySpeedMultiplier = origSpeed;
        }
    }
}

// Helper component for basement lamp proximity
public class BasementLampStatus : MonoBehaviour
{
    public bool nearLamp = false;
}