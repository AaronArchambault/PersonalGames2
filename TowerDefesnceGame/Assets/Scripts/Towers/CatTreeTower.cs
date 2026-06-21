using UnityEngine;
using System.Collections;

public class CatTreeTower : Tower
{
    [Header("Cat Tree Income")]
    public float goldInterval  = 5f;    // seconds between gold generation
    public int   goldPerTick   = 15;    // gold earned each tick
    public float boostRadius   = 2.5f;  // nearby towers get a small boost

    [Header("Sleeping")]
    public ParticleSystem zzzParticles;
    public SpriteRenderer sleepingCatSprite;

    [Header("Wake Up")]
    public float wakeRadius    = 3f;    // wakes up if enemies are this close
    public float wakeFireRate  = 2f;    // fire rate when awake (scratches enemies)
    public string scratchTag   = "HitEffect";

    private bool isSleeping    = true;
    private float goldTimer    = 0f;

    protected override void Start()
    {
        base.Start();
        SetSleeping(true);
    }

    protected override void Update()
    {
        // Check for nearby enemies — wake up if any are close
        bool enemiesNear = Physics2D.OverlapCircle(
            transform.position, wakeRadius, LayerMask.GetMask("Enemy"));

        if (enemiesNear && isSleeping)   SetSleeping(false);
        if (!enemiesNear && !isSleeping) SetSleeping(true);

        // Gold tick
        goldTimer += Time.deltaTime;
        if (goldTimer >= goldInterval)
        {
            goldTimer = 0f;
            EarnIncome();
        }

        // Only attack when awake
        if (!isSleeping) base.Update();
    }

    protected override void Shoot()
    {
        // Cat wakes up and scratches nearby enemies
        if (currentTarget == null) return;
        currentTarget.GetComponent<Enemy>()?.TakeDamage(Damage);
        ObjectPool.Instance.Spawn(scratchTag, currentTarget.position, Quaternion.identity);
        FloatingTextPool.Instance?.Spawn(
            currentTarget.position + Vector3.up, "SCRATCH!", Color.yellow);
    }

    void EarnIncome()
    {
        GameManager.Instance.EarnGold(goldPerTick);
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.6f,
            $"+{goldPerTick}g", Color.yellow);
        AudioManager.Instance?.Play("coin_earn");
    }

    void SetSleeping(bool sleeping)
    {
        isSleeping = sleeping;
        if (zzzParticles)
        {
            if (sleeping) zzzParticles.Play();
            else zzzParticles.Stop();
        }
        if (sleepingCatSprite)
            sleepingCatSprite.color = sleeping
                ? new Color(1f, 1f, 1f, 0.7f)
                : Color.white;
    }
}