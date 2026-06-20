using UnityEngine;

public class CrowEnemy : Enemy
{
    [Header("Gold Theft")]
    public int goldStealAmount = 10;
    public float stealRadius   = 1.5f;
    private bool hasStolen     = false;

    protected override void Update()
    {
        base.Update();
        TryStealGold();
    }

    void TryStealGold()
    {
        if (hasStolen) return;
        if (GameManager.Instance.Gold < goldStealAmount) return;

        // Steal once when passing through the path
        if (waypointIndex > 1)
        {
            hasStolen = true;
            GameManager.Instance.SpendGold(goldStealAmount);
            FloatingTextPool.Instance?.Spawn(
                transform.position + Vector3.up,
                $"-{goldStealAmount}g STOLEN!", Color.red);
            AudioManager.Instance?.Play("gold_stolen");
        }
    }

    protected override void Die()
    {
        // Drops some gold back on death
        if (hasStolen)
            GameManager.Instance.EarnGold(goldStealAmount / 2);
        base.Die();
    }
}