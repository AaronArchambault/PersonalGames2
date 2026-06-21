
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
 
    public void ActivatePowerUp(string id)
    {
        switch (id)
        {
            case "yarn_nuke":       StartCoroutine(YarnNuke());       break;
            case "catnip_bomb":     StartCoroutine(CatnipBomb());     break;
            case "tuna_bribe":      StartCoroutine(TunaBribe());      break;
            case "extra_life":      ExtraLife();                      break;
            case "tower_refresh":   TowerRefresh();                   break;
            case "speed_nap":       StartCoroutine(SpeedNap());       break;
            case "scratching_post": GoldenScratchingPost();           break;
            default: Debug.LogWarning($"Unknown power-up: {id}");    break;
        }
    }
 
    // ── YARN NUKE ─────────────────────────────────────────────
    IEnumerator YarnNuke()
    {
        UIManager.Instance?.Announce("YARN NUKE!", new Color(1f, 0.4f, 0.8f));
        CameraShake.Instance?.Shake(0.25f, 0.4f);
        AudioManager.Instance?.Play("powerup_yarn");
 
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            e.ApplySlow(0.999f, 2f);
            var sr = e.GetComponent<SpriteRenderer>();
            if (sr) sr.color = new Color(1f, 0.6f, 0.9f);
        }
 
        ObjectPool.Instance.Spawn("Explosion",
            Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f)),
            Quaternion.identity);
 
        yield return new WaitForSeconds(2f);
 
        var stillAlive = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in stillAlive)
        {
            if (e == null) continue;
            var sr = e.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }
    }
 
    // ── CATNIP BOMB ───────────────────────────────────────────
    IEnumerator CatnipBomb()
    {
        UIManager.Instance?.Announce("CATNIP BOMB!", new Color(0.5f, 1f, 0.3f));
        CameraShake.Instance?.Shake(0.2f, 0.3f);
        AudioManager.Instance?.Play("powerup_catnip");
 
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
 
        foreach (var e in enemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            int newWP = Mathf.Max(0, e.GetWaypointIndex() - 4);
            e.SetWaypointIndex(newWP);
            e.ApplySlow(0.5f, 3f);
            var sr = e.GetComponent<SpriteRenderer>();
            if (sr) sr.color = new Color(0.5f, 1f, 0.4f);
            FloatingTextPool.Instance?.Spawn(
                e.transform.position + Vector3.up, "LOOPY!", Color.green);
        }
 
        yield return new WaitForSeconds(3f);
 
        var stillAlive = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in stillAlive)
        {
            if (e == null) continue;
            var sr = e.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }
    }
 
    // ── TUNA BRIBE ────────────────────────────────────────────
    IEnumerator TunaBribe()
    {
        UIManager.Instance?.Announce("TUNA BRIBE! Enemies frozen!", Color.cyan);
        AudioManager.Instance?.Play("powerup_tuna");
 
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            e.ApplySlow(0.9999f, 1.5f);
            var sr = e.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.cyan;
        }
 
        StartCoroutine(FlashOverlay(Color.cyan, 0.15f));
        yield return new WaitForSeconds(1.5f);
 
        var stillAlive = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in stillAlive)
        {
            if (e == null) continue;
            var sr = e.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }
    }
 
    // ── EXTRA LIFE ────────────────────────────────────────────
    void ExtraLife()
    {
        // AddLives handles both the increment and the event invoke
        GameManager.Instance.AddLives(5);
        UIManager.Instance?.Announce("+5 LIVES!", Color.green);
        StartCoroutine(FlashOverlay(Color.green, 0.2f));
        AudioManager.Instance?.Play("powerup_life");
    }
 
    // ── TOWER REFRESH ─────────────────────────────────────────
    void TowerRefresh()
    {
        UIManager.Instance?.Announce("TOWER REFRESH! Fire at will!", Color.yellow);
        AudioManager.Instance?.Play("powerup_refresh");
 
        var towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (var t in towers)
        {
            if (t == null) continue;
            t.ResetFireCooldown();
            StartCoroutine(FlashTower(t));
        }
    }
 
    IEnumerator FlashTower(Tower t)
    {
        if (t == null) yield break;
        var sr = t.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        sr.color = Color.yellow;
        yield return new WaitForSeconds(0.15f);
        if (sr) sr.color = Color.white;
    }
 
    // ── SPEED NAP ─────────────────────────────────────────────
    IEnumerator SpeedNap()
    {
        UIManager.Instance?.Announce("SPEED NAP! Towers overdrive!", Color.yellow);
        AudioManager.Instance?.Play("powerup_speed");
 
        var towers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (var t in towers)
            if (t != null) t.ApplyFireRateBoost(2f, 10f);
 
        foreach (var t in towers)
        {
            var sr = t?.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.yellow;
        }
 
        yield return new WaitForSeconds(10f);
 
        var stillThere = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (var t in stillThere)
        {
            var sr = t?.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }
    }
 
    // ── GOLDEN SCRATCHING POST ────────────────────────────────
    void GoldenScratchingPost()
    {
        GameManager.Instance.EarnGold(50);
        UIManager.Instance?.Announce("+50g Golden Scratching Post!", Color.yellow);
        AudioManager.Instance?.Play("coin_earn");
        StartCoroutine(FlashOverlay(Color.yellow, 0.1f));
    }
 
    // ── HELPERS ───────────────────────────────────────────────
    IEnumerator FlashOverlay(Color color, float duration)
    {
        if (BrightnessOverlay.Instance == null) yield break;
        BrightnessOverlay.Instance.SetBrightness(1.5f);
        yield return new WaitForSeconds(duration);
        BrightnessOverlay.Instance.SetBrightness(1f);
    }
}