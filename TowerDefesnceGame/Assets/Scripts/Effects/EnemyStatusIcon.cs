
using UnityEngine;
 
public class EnemyStatusIcons : MonoBehaviour
{
    [Header("Icon Sprites")]
    public SpriteRenderer slowIcon;    // blue snowflake sprite child
    public SpriteRenderer wrappedIcon; // pink yarn icon child
    public SpriteRenderer eliteIcon;   // gold star icon child
 
    private Enemy enemy;
 
    void Awake() => enemy = GetComponentInParent<Enemy>();
 
    void Update()
    {
        if (enemy == null) return;
        if (slowIcon)    slowIcon.enabled    = enemy.IsSlowed && !enemy.IsWrapped;
        if (wrappedIcon) wrappedIcon.enabled = enemy.IsWrapped;
        if (eliteIcon)   eliteIcon.enabled   = enemy.IsElite;
    }
}
 
// Setup: Add as child GO of each enemy prefab
// Position at (0, 0.65, 0) above the health bar
// Add 3 child SpriteRenderers and assign them in Inspector
 