using UnityEngine;
using System.Collections;

public class CardboardBoxCat : Tower
{
    [Header("Cardboard Box")]
    public float    pounceRange      = 1.5f;  // triggers when enemy is this close
    public float    pounceDamage     = 80f;
    public float    pounceKnockback  = 3f;    // pushes enemy back in path
    public float    rechargeTime     = 4f;    // time before it hides again
    public int      pounceKnockbackWaypoints = 2; // knocks enemy back this many waypoints

    [Header("Visuals")]
    public GameObject boxSprite;     // the cardboard box (shows when hiding)
    public GameObject catSprite;     // the cat (shows when pouncing)
    public ParticleSystem poofEffect;
    public string pounceEffectTag = "Explosion";

    private bool isHiding   = true;
    private bool onCooldown = false;

    protected override void Start()
    {
        base.Start();
        SetHidingState(true);
    }

    protected override void Update()
    {
        if (onCooldown || GameManager.Instance.GameOver) return;

        if (isHiding)
        {
            // Check if any enemy is close enough to trigger pounce
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, pounceRange, LayerMask.GetMask("Enemy"));
            foreach (var hit in hits)
            {
                var e = hit.GetComponent<Enemy>();
                if (e != null) { StartCoroutine(Pounce(e)); break; }
            }
        }
    }

    protected override void Shoot() { } // Pounce handles damage directly

    IEnumerator Pounce(Enemy target)
    {
        onCooldown = true;
        SetHidingState(false);

        // Poof effect
        poofEffect?.Play();

        // Knockback — push enemy back along path
        if (target != null)
        {
            target.TakeDamage(pounceDamage + Damage);
            int newWP = Mathf.Max(0,
                target.GetWaypointIndex() - pounceKnockbackWaypoints);
            target.SetWaypointIndex(newWP);

            // Move target back to that waypoint position
            if (newWP < WaypointManager.Instance.waypoints.Length)
                target.transform.position =
                    WaypointManager.Instance.waypoints[newWP].position;
        }

        ObjectPool.Instance.Spawn(pounceEffectTag, transform.position, Quaternion.identity);
        FloatingTextPool.Instance?.Spawn(transform.position + Vector3.up, "POUNCE!", Color.yellow);

        // Wait then hide again
        yield return new WaitForSeconds(rechargeTime);
        SetHidingState(true);

        yield return new WaitForSeconds(1f);
        onCooldown = false;
    }

    void SetHidingState(bool hiding)
    {
        isHiding = hiding;
        if (boxSprite) boxSprite.SetActive(hiding);
        if (catSprite) catSprite.SetActive(!hiding);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pounceRange);
    }
}