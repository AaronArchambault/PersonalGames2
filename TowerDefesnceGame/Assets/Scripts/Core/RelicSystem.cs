// Scripts/Core/RelicSystem.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Relic
{
    public string id;
    public string name;
    public string description;
    public Sprite icon;

    // What it does (applied globally)
    public float globalDamageBonus;
    public float globalRangeBonus;
    public float globalGoldBonus;
    public int   startingLivesBonus;
}

public class RelicSystem : MonoBehaviour
{
    public static RelicSystem Instance { get; private set; }

    public List<Relic> allRelics = new();
    public List<Relic> activeRelics = new();

    public float TotalDamageBonus => GetTotal(r => r.globalDamageBonus);
    public float TotalRangeBonus  => GetTotal(r => r.globalRangeBonus);
    public float TotalGoldBonus   => GetTotal(r => r.globalGoldBonus);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GrantRandomRelic()
    {
        if (allRelics.Count == 0) return;
        var available = allRelics.FindAll(r => !activeRelics.Contains(r));
        if (available.Count == 0) return;

        var relic = available[Random.Range(0, available.Count)];
        activeRelics.Add(relic);

        UIManager.Instance?.Announce($"RELIC: {relic.name}!", Color.yellow);
        FloatingTextPool.Instance?.Spawn(
            Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.6f, 10f)),
            relic.description, Color.yellow);

        // Re-apply all tower stats with new bonus
        foreach (var tower in FindObjectsByType<Tower>(FindObjectsSortMode.None))
            tower.RecalculateStats();
    }

    float GetTotal(System.Func<Relic, float> selector)
    {
        float total = 0;
        foreach (var r in activeRelics) total += selector(r);
        return total;
    }
}