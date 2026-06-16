using UnityEngine;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Returns true if the merge was executed
    public bool TryMerge(CowEntity dragger, CowEntity target)
    {
        if (dragger == null || target == null)          return false;
        if (dragger == target)                          return false;
        if (dragger.Data.tier != target.Data.tier)     return false;

        CowData nextData = GameManager.Instance.database.GetNextTier(dragger.Data);
        if (nextData == null)
        {
            // Already at max tier — no merge possible
            UIManager.Instance?.ShowMessage("Maximum evolution reached!");
            return false;
        }

        GridCell mergeCell = target.CurrentCell;

        // Play effects at target position
        PlayMergeEffects(target.Data, mergeCell.worldPosition);

        // Award merge bonus
        GameManager.Instance.AddCoins(target.Data.mergeReward);

        // Remove both cows
        dragger.CurrentCell.Occupant = null;
        target.CurrentCell.Occupant  = null;
        Destroy(dragger.gameObject);
        Destroy(target.gameObject);

        // Spawn evolved cow at merge location
        GridManager.Instance.SpawnCow(nextData.tier, mergeCell.x, mergeCell.y);

        UIManager.Instance?.ShowEvolutionPopup(nextData);
        return true;
    }

    void PlayMergeEffects(CowData data, Vector3 position)
    {
        if (data.mergeParticlePrefab)
            Destroy(Instantiate(data.mergeParticlePrefab, position, Quaternion.identity), 3f);

        if (data.mergeSound)
        {
            var go = new GameObject("MergeSFX");
            go.transform.position = position;
            var src = go.AddComponent<AudioSource>();
            src.PlayOneShot(data.mergeSound);
            Destroy(go, data.mergeSound.length + 0.1f);
        }
    }
}
