using UnityEngine;

public class ExplosionEffect : MonoBehaviour, IPoolable
{
    public string poolTag = "Explosion";
    public ParticleSystem particles;
    public float lifetime = 1f;

    public void OnCreated(string tag) => poolTag = tag;
    public void OnDespawn() { }
    public void OnSpawn()
    {
        particles?.Play();
        Invoke(nameof(ReturnToPool), lifetime);
    }

    void ReturnToPool()
    {
        CancelInvoke();
        particles?.Stop();
        ObjectPool.Instance.Despawn(poolTag, gameObject);
    }
}