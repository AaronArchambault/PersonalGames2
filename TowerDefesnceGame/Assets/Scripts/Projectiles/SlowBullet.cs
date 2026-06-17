using UnityEngine;

public class SlowBullet : MonoBehaviour, IPoolable
{
    public string poolTag = "SlowBullet";
    public float  speed   = 7f;

    private Transform target;
    private float slowFactor;
    private float slowDuration;
    private bool done;

    public void OnCreated(string tag) => poolTag = tag;
    public void OnSpawn()  => done = false;
    public void OnDespawn(){ target = null; done = false; }

    public void Setup(Transform t, float factor, float duration)
    {
        target = t;
        slowFactor = factor;
        slowDuration = duration;
    }

    void Update()
    {
        if (done) return;
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Despawn(); return;
        }
        transform.position = Vector2.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.15f)
        {
            target.GetComponent<Enemy>()?.ApplySlow(slowFactor, slowDuration);
            Despawn();
        }
    }

    void Despawn()
    {
        done = true;
        ObjectPool.Instance.Despawn(poolTag, gameObject);
    }
}