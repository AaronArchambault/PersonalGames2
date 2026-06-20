using UnityEngine;
using System.Collections;

public class NinjaMouse : Enemy
{
    [Header("Ninja Mouse")]
    public float invisDuration  = 2f;   // how long it stays invisible
    public float visibleDuration = 1f;  // how long it's visible between stealth phases
    public float stealthDamageMultiplier = 0f; // can't be targeted while invisible

    private bool isInvisible = false;

    public override void OnSpawn()
    {
        base.OnSpawn();
        StartCoroutine(StealthCycle());
    }

    IEnumerator StealthCycle()
    {
        yield return new WaitForSeconds(visibleDuration);
        while (true)
        {
            // Go invisible
            isInvisible = true;
            if (sr) sr.color = new Color(1f, 1f, 1f, 0.2f);
            // Tag as invisible so towers skip targeting
            gameObject.layer = LayerMask.NameToLayer("Default");
            yield return new WaitForSeconds(invisDuration);

            // Become visible
            isInvisible = false;
            if (sr) sr.color = Color.white;
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            yield return new WaitForSeconds(visibleDuration);
        }
    }

    public override void TakeDamage(float amount)
    {
        // Can't be damaged while invisible
        if (isInvisible) return;
        base.TakeDamage(amount);
    }
}