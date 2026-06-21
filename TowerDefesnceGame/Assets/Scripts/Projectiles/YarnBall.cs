using UnityEngine;
using System.Collections;

public class YarnBall : MonoBehaviour, IPoolable
{
    public string poolTag  = "YarnBall";
    public float  speed    = 8f;
    public string hitEffectTag = "HitEffect";

    private Transform target;
    private float     damage;
    private float     wrapDuration;
    private float     slowAfter;
    private bool      done;

    public void OnCreated(string tag) => poolTag = tag;
    public void OnSpawn()  => done = false;
    public void OnDespawn(){ target = null; done = false; }

    public void Setup(Transform t, float dmg, float wrap, float slow)
    {
        target       = t;
        damage       = dmg;
        wrapDuration = wrap;
        slowAfter    = slow;
    }

    void Update()
    {
        if (done) return;
        if (target == null || !target.gameObject.activeInHierarchy)
        { Despawn(); return; }

        transform.position = Vector2.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime);
        transform.Rotate(0, 0, 400f * Time.deltaTime); // spin the yarn ball

        if (Vector2.Distance(transform.position, target.position) < 0.15f)
            Hit();
    }

    void Hit()
    {
        var e = target.GetComponent<Enemy>();
        if (e != null)
        {
            e.TakeDamage(damage);
            StartCoroutine(WrapEnemy(e));
        }
        ObjectPool.Instance.Spawn(hitEffectTag, transform.position, Quaternion.identity);
        Despawn();
    }

    IEnumerator WrapEnemy(Enemy e)
    {
        // Fully stop the enemy
        e.ApplySlow(0.999f, wrapDuration);
        FloatingTextPool.Instance?.Spawn(
            e.transform.position + Vector3.up, "WRAPPED!", new Color(1f, 0.4f, 0.8f));

        // Tint enemy pink (yarn color)
        var sr = e.GetComponent<SpriteRenderer>();
        Color orig = sr != null ? sr.color : Color.white;
        if (sr) sr.color = new Color(1f, 0.6f, 0.9f);

        yield return new WaitForSeconds(wrapDuration);

        // Apply lingering slow after wrap
        if (e != null && e.gameObject.activeInHierarchy)
        {
            e.ApplySlow(slowAfter, 2f);
            if (sr) sr.color = orig;
        }
    }

    void Despawn()
    {
        done = true;
        ObjectPool.Instance.Despawn(poolTag, gameObject);
    }
}