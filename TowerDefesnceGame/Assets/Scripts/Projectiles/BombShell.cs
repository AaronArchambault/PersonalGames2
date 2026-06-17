using UnityEngine;

public class BombShell : MonoBehaviour, IPoolable
{
    public string poolTag = "BombShell";
    public float  speed   = 6f;

    private Vector3 targetPos;
    private float   damage;
    private float   radius;
    private string  explosionTag;
    private bool    done;

    public void OnCreated(string tag) => poolTag = tag;
    public void OnSpawn()  => done = false;
    public void OnDespawn(){ done = false; }

    public void Setup(Vector3 target, float dmg, float aoeRadius, string explTag)
    {
        targetPos    = target;
        damage       = dmg;
        radius       = aoeRadius;
        explosionTag = explTag;
    }

    void Update()
    {
        if (done) return;

        // Arc travel — simple lerp toward target
        transform.position = Vector2.MoveTowards(
            transform.position, targetPos, speed * Time.deltaTime);

        // Spin shell
        transform.Rotate(0, 0, 300f * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
            Explode();
    }

    void Explode()
    {
        done = true;

        // Spawn explosion effect
        ObjectPool.Instance.Spawn(explosionTag, targetPos, Quaternion.identity);

        // AoE damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, radius, LayerMask.GetMask("Enemy"));
        foreach (var h in hits)
            h.GetComponent<Enemy>()?.TakeDamage(damage);

        // Camera shake
        CameraShake.Instance?.Shake(0.2f, 0.25f);

        ObjectPool.Instance.Despawn(poolTag, gameObject);
    }
}