using UnityEngine;
using System.Collections;

public class MegaRoomba : Enemy
{
    [Header("Mega Roomba Boss")]
    public float shieldHP         = 500f;
    public float shieldRegenRate  = 20f;   // regens per second when no damage
    public float regenDelay       = 3f;    // seconds after last hit before regen starts
    public string shieldBreakTag  = "Explosion";
    public string bossIntroText   = "THE MEGA ROOMBA APPROACHES...";

    private float currentShieldHP;
    private float lastDamageTime;
    private bool  shieldActive = true;

    [Header("Phase 2")]
    public float phase2SpeedBoost = 1.5f;
    private bool enteredPhase2 = false;

    public override void OnSpawn()
    {
        base.OnSpawn();
        currentShieldHP  = shieldHP;
        lastDamageTime   = -999f;
        shieldActive     = true;
        enteredPhase2    = false;

        // Boss intro announcement
        UIManager.Instance?.Announce(bossIntroText, Color.red);
        CameraShake.Instance?.Shake(0.3f, 0.5f);
    }

    protected override void Update()
    {
        base.Update();
        HandleShieldRegen();
        CheckPhase2();
    }

    void HandleShieldRegen()
    {
        if (!shieldActive) return;
        if (Time.time - lastDamageTime < regenDelay) return;
        currentShieldHP = Mathf.Min(shieldHP,
            currentShieldHP + shieldRegenRate * Time.deltaTime);
    }

    void CheckPhase2()
    {
        if (enteredPhase2) return;
        if (currentHealth < maxHealth * 0.5f)
        {
            enteredPhase2 = true;
            moveSpeed *= phase2SpeedBoost;
            UIManager.Instance?.Announce("MEGA ROOMBA: TURBO MODE!", Color.red);
            CameraShake.Instance?.Shake(0.4f, 0.6f);
            if (sr) sr.color = new Color(1f, 0.3f, 0.3f);
            GetComponent<EnemyAnimator>()?.SetPhase2(true);
        }
    }

    public override void TakeDamage(float amount)
    {
        lastDamageTime = Time.time;

        if (shieldActive && currentShieldHP > 0)
        {
            currentShieldHP -= amount;
            FloatingTextPool.Instance?.Spawn(
                transform.position + Vector3.up, $"SHIELD: {Mathf.RoundToInt(currentShieldHP)}", Color.cyan);
            if (currentShieldHP <= 0)
            {
                shieldActive = false;
                ObjectPool.Instance.Spawn(shieldBreakTag, transform.position, Quaternion.identity);
                CameraShake.Instance?.Shake(0.3f, 0.4f);
                FloatingTextPool.Instance?.Spawn(
                    transform.position + Vector3.up * 0.5f, "SHIELD BROKEN!", Color.yellow);
            }
            return; // shield absorbs all damage
        }
        base.TakeDamage(amount);
    }

    protected override void Die()
    {
        // Big death sequence
        StartCoroutine(BossDeath());
    }

    IEnumerator BossDeath()
    {
        UIManager.Instance?.Announce("MEGA ROOMBA DEFEATED!", Color.green);
        CameraShake.Instance?.Shake(0.5f, 1f);
        GameManager.Instance.EarnGold(200);

        for (int i = 0; i < 5; i++)
        {
            ObjectPool.Instance.Spawn(shieldBreakTag,
                transform.position + (Vector3)Random.insideUnitCircle * 0.5f,
                Quaternion.identity);
            yield return new WaitForSeconds(0.15f);
        }

        base.Die();
    }
}