using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    public string poolTag = "Bullet";
    public float  speed   = 10f;
    public string hitEffectTag = "HitEffect";

    private Transform target;
    private float     damage;
    private bool      directional = false;
    private Vector3   travelDir;
    private bool      isDone;

    public void OnCreated(string tag) => poolTag = tag;
    public void OnDespawn()
    {
        target = null;
        isDone = false;
        directional = false;
    }
    public void OnSpawn()
    {
        isDone = false;
    }

    // Homing
    public void Setup(Transform t, float dmg)
    {
        target = t;
        damage = dmg;
        directional = false;
    }

    // Directional (sprayer)
    public void SetupDirectional(Vector3 dir, float dmg)
    {
        directional = true;
        travelDir = dir.normalized;
        damage = dmg;
    }

    void Update()
    {
        if (isDone) return;

        if (directional)
        {
            transform.position += travelDir * speed * Time.deltaTime;
            // Despawn after travelling a distance
            if (Vector3.Distance(transform.position, Vector3.zero) > 30f)
                Despawn();
            return;
        }

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Despawn(); return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.15f)
            Hit();

           // In Bullet.Update() — apply wind when in Rooftop level
if (LevelThemeManager.HasWind)
{
    Vector2 wind = LevelThemeManager.WindForce * Time.deltaTime * 0.3f;
    transform.position += (Vector3)wind;
} 
    }


private Tower sourceTower;

public void Setup(Transform t, float dmg, Tower source = null)
{
    target      = t;
    damage      = dmg;
    sourceTower = source;
}

void Hit()
{
    var e = target?.GetComponent<Enemy>();
    if (e != null)
    {
        e.TakeDamage(damage);
        // If enemy dies, credit kill to tower
        if (e.GetHealth() <= 0 && sourceTower != null)
            sourceTower.killCount++;
    }
    ObjectPool.Instance.Spawn(hitEffectTag, transform.position, Quaternion.identity);
    Despawn();
}
    /*void Hit()
    {
        target?.GetComponent<Enemy>()?.TakeDamage(damage);
        ObjectPool.Instance.Spawn(hitEffectTag, transform.position, Quaternion.identity);
        Despawn();
    }*/

    void Despawn()
    {
        isDone = true;
        ObjectPool.Instance.Despawn(poolTag, gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!directional) return;
        if (!other.CompareTag("Enemy")) return;
        other.GetComponent<Enemy>()?.TakeDamage(damage);
        ObjectPool.Instance.Spawn(hitEffectTag, transform.position, Quaternion.identity);
        Despawn();
    }
}