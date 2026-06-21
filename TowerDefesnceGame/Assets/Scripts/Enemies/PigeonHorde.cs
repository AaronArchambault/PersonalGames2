using UnityEngine;

public class PigeonHorde : Enemy
{
    // PigeonHorde enemies spawn together via WaveManager
    // Each pigeon has very low HP and reward
    // Inspector values: HP: 20, Speed: 3, Reward: 3, LiveDamage: 1

    [Header("Pigeon Horde")]
    public bool poopsOnTowers = true;
    public float poopRadius   = 0.5f;

    protected override void Move()
    {
        base.Move();

        // Chance to poop on a tower below (cosmetic)
        if (poopsOnTowers && Random.value < 0.001f)
            CheckPoopOnTower();
    }

    void CheckPoopOnTower()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position, poopRadius, LayerMask.GetMask("Tower"));
        if (hit == null) return;
        FloatingTextPool.Instance?.Spawn(
            transform.position, "💩", Color.white);
    }
}