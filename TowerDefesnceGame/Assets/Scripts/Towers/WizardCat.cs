using UnityEngine;
using System.Collections;

public class WizardCat : Tower
{
    public enum SpellType
    {
        Fireball,    // AoE damage
        Freeze,      // mass slow
        Polymorph,   // turns enemy into mouse (slow, low HP override)
        TimeStop,    // briefly freezes all enemies
        GoldRain,    // damages and gives bonus gold
        Confusion    // reverses enemy direction briefly
    }

    [Header("Wizard")]
    public string spellEffectTag = "Explosion";
    public string iceEffectTag   = "SlowBullet";
    public float  polyTime       = 3f;    // how long polymorph lasts
    public float  timeStopTime   = 1.5f;

    private SpellType lastSpell = SpellType.Fireball;

    protected override void Shoot()
    {
        if (currentTarget == null) return;

        // Pick a random spell (never the same twice in a row)
        SpellType spell;
        do { spell = (SpellType)Random.Range(0, 6); }
        while (spell == lastSpell);
        lastSpell = spell;

        StartCoroutine(CastSpell(spell));
    }

    IEnumerator CastSpell(SpellType spell)
    {
        // Wind-up
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up * 0.8f,
            GetSpellName(spell), GetSpellColor(spell));

            GetComponent<TowerAnimator>()?.PlayCast();

        yield return new WaitForSeconds(0.3f);

        switch (spell)
        {
            case SpellType.Fireball:
                CastFireball(); break;
            case SpellType.Freeze:
                CastFreeze(); break;
            case SpellType.Polymorph:
                CastPolymorph(); break;
            case SpellType.TimeStop:
                yield return StartCoroutine(CastTimeStop()); break;
            case SpellType.GoldRain:
                CastGoldRain(); break;
            case SpellType.Confusion:
                CastConfusion(); break;
        }
    }

    void CastFireball()
    {
        ObjectPool.Instance.Spawn(spellEffectTag, currentTarget.position, Quaternion.identity);
        CameraShake.Instance?.Shake(0.15f, 0.2f);
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            currentTarget.position, 2f, LayerMask.GetMask("Enemy"));
        foreach (var h in hits)
            h.GetComponent<Enemy>()?.TakeDamage(Damage * 2f);
    }

    void CastFreeze()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, Range, LayerMask.GetMask("Enemy"));
        foreach (var h in hits)
            h.GetComponent<Enemy>()?.ApplySlow(0.8f, 3f);
    }

    void CastPolymorph()
    {
        var e = currentTarget?.GetComponent<Enemy>();
        if (e == null) return;
        // Polymorph: max out slow and deal damage over time
        e.ApplySlow(0.9f, polyTime);
        e.TakeDamage(Damage * 3f);
        FloatingTextPool.Instance?.Spawn(
            currentTarget.position + Vector3.up, "POLYMORPH!", Color.magenta);
    }

    IEnumerator CastTimeStop()
    {
        // Slow time for all enemies briefly
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, Range * 1.5f, LayerMask.GetMask("Enemy"));
        foreach (var h in hits)
            h.GetComponent<Enemy>()?.ApplySlow(0.99f, timeStopTime);

        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up, "TIME STOP!", Color.white);
        yield return new WaitForSeconds(timeStopTime);
    }

    void CastGoldRain()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            currentTarget.position, 2f, LayerMask.GetMask("Enemy"));
        int bonusGold = 0;
        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e == null) continue;
            e.TakeDamage(Damage);
            bonusGold += 5;
        }
        GameManager.Instance.EarnGold(bonusGold);
        FloatingTextPool.Instance?.Spawn(
            transform.position + Vector3.up, $"+{bonusGold}g MAGIC!", Color.yellow);
    }

    void CastConfusion()
    {
        // Knock enemy back along path
        var e = currentTarget?.GetComponent<Enemy>();
        if (e == null) return;
        int newWP = Mathf.Max(0, e.GetWaypointIndex() - 3);
        e.SetWaypointIndex(newWP);
        e.TakeDamage(Damage * 0.5f);
        FloatingTextPool.Instance?.Spawn(
            currentTarget.position + Vector3.up, "CONFUSED!", Color.magenta);
    }

    string GetSpellName(SpellType s) => s switch
    {
        SpellType.Fireball  => "FIREBALL!",
        SpellType.Freeze    => "FREEZE!",
        SpellType.Polymorph => "POLYMORPH!",
        SpellType.TimeStop  => "TIME STOP!",
        SpellType.GoldRain  => "GOLD RAIN!",
        SpellType.Confusion => "CONFUSION!",
        _ => "???"
    };

    Color GetSpellColor(SpellType s) => s switch
    {
        SpellType.Fireball  => Color.red,
        SpellType.Freeze    => Color.cyan,
        SpellType.Polymorph => Color.magenta,
        SpellType.TimeStop  => Color.white,
        SpellType.GoldRain  => Color.yellow,
        SpellType.Confusion => new Color(0.8f, 0.4f, 1f),
        _ => Color.white
    };
}