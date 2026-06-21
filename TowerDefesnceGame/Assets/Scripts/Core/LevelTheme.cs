using UnityEngine;

public enum ThemeType
{
    Normal,
    LivingRoom,
    Kitchen,
    Garden,
    Rooftop,
    Basement,
    VetOffice
}

[CreateAssetMenu(fileName = "LevelTheme", menuName = "Tower Defense/Level Theme")]
public class LevelTheme : ScriptableObject
{
    [Header("Identity")]
    public ThemeType themeType;
    public string    themeName;
    [TextArea(2, 4)]
    public string    themeDescription;
    public Color     themeColor = Color.white;
    public Sprite    themeIcon;

    [Header("Tower Modifiers")]
    public float towerCostMultiplier    = 1f;   // VetOffice: 1.2
    public float towerDamageMultiplier  = 1f;   // VetOffice: 1.2
    public float towerRangeMultiplier   = 1f;   // Basement: 0.6 near lamps: 1.0
    public float towerFireRateMultiplier = 1f;

    [Header("Enemy Modifiers")]
    public float enemySpeedMultiplier   = 1f;
    public float enemyHealthMultiplier  = 1f;

    [Header("Projectile Modifiers")]
    public Vector2 windForce            = Vector2.zero; // Rooftop
    public float   windVariance         = 0.2f;

    [Header("Special Rules")]
    public bool  hasLampSystem          = false; // Basement
    public bool  hasBirdbathAttraction  = false; // Garden
    public bool  hasWindEffect          = false; // Rooftop
    public bool  hasTVBeacon            = false; // LivingRoom
    public bool  hasFridgeBonus         = false; // Kitchen

    [Header("Background & Visuals")]
    public Color ambientLightColor      = Color.white;
    public float ambientLightIntensity  = 1f;
    public Sprite backgroundSprite;
}