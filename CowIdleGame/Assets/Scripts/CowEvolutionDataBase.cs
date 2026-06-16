using UnityEngine;

// Create via: Assets > Create > CowEvolution > Evolution Database
// Drag your CowData assets into the 'tiers' list in order (tier 0 first)
[CreateAssetMenu(fileName = "EvolutionDatabase", menuName = "CowEvolution/Evolution Database")]
public class CowEvolutionDatabase : ScriptableObject
{
    [Tooltip("Ordered list of CowData from tier 0 (basic) to max tier")]
    public CowData[] tiers;

    public CowData GetTier(int tier)
    {
        if (tier < 0 || tier >= tiers.Length) return null;
        return tiers[tier];
    }

    public CowData GetNextTier(CowData current)
    {
        int next = current.tier + 1;
        return GetTier(next);
    }

    public int TierCount => tiers.Length;
}
