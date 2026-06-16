using UnityEngine;

// Create via: Assets > Create > CowEvolution > Cow Data
[CreateAssetMenu(fileName = "NewCow", menuName = "CowEvolution/Cow Data")]
public class CowData : ScriptableObject
{
    [Header("Identity")]
    public int tier;                  // 0 = basic cow, 7 = god cow
    public string cowName;            // "Cosmic Udder", "Volcano Moo", etc.
    public Sprite sprite;             // assign in Inspector
    public Color tintColor = Color.white;

    [Header("Economy")]
    public float coinsPerSecond;      // passive income while on board
    public int mergeReward;           // bonus coins awarded on successful merge

    [Header("Visual FX")]
    public GameObject mergeParticlePrefab;   // optional burst on merge
    public AudioClip mergeSound;
}
