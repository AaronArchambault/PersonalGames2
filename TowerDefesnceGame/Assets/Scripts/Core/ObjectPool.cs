using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 10;
    }

    public List<Pool> pools = new();

    private Dictionary<string, Queue<GameObject>> poolDict = new();
    private Dictionary<string, Pool> poolLookup = new();

    /*void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        foreach (var pool in pools)
        {
            poolLookup[pool.tag] = pool;
            var queue = new Queue<GameObject>();
            for (int i = 0; i < pool.initialSize; i++)
                queue.Enqueue(CreateNew(pool));
            poolDict[pool.tag] = queue;
        }
    }*/
    void Awake()
{
    if (Instance == null) Instance = this;
    else { Destroy(gameObject); return; }

    foreach (var pool in pools)
    {
        // SKIP pools with no prefab assigned
        if (pool.prefab == null)
        {
            Debug.LogWarning($"[ObjectPool] Pool '{pool.tag}' has no prefab assigned — skipping.");
            poolDict[pool.tag] = new Queue<GameObject>();
            poolLookup[pool.tag] = pool;
            continue;
        }

        poolLookup[pool.tag] = pool;
        var queue = new Queue<GameObject>();
        for (int i = 0; i < pool.initialSize; i++)
            queue.Enqueue(CreateNew(pool));
        poolDict[pool.tag] = queue;
    }
}

    GameObject CreateNew(Pool pool)
    {
        var obj = Instantiate(pool.prefab, transform);
        obj.SetActive(false);
        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnCreated(pool.tag);
        return obj;
    }

    public GameObject Spawn(string tag, Vector3 pos, Quaternion rot)
    {
        if (!poolDict.TryGetValue(tag, out var queue))
        {
            Debug.LogWarning($"[ObjectPool] Tag '{tag}' not found.");
            return null;
        }

        GameObject obj = queue.Count > 0
            ? queue.Dequeue()
            : CreateNew(poolLookup[tag]);

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);
        obj.GetComponent<IPoolable>()?.OnSpawn();
        return obj;
    }

    public void Despawn(string tag, GameObject obj, float delay = 0f)
    {
        if (delay > 0)
            StartCoroutine(DespawnDelayed(tag, obj, delay));
        else
            DespawnImmediate(tag, obj);
    }

    System.Collections.IEnumerator DespawnDelayed(string tag, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        DespawnImmediate(tag, obj);
    }

    void DespawnImmediate(string tag, GameObject obj)
    {
        obj.GetComponent<IPoolable>()?.OnDespawn();
        obj.SetActive(false);
        if (poolDict.TryGetValue(tag, out var queue))
            queue.Enqueue(obj);
    }

    public GameObject GetPrefabByTag(string tag) =>
        poolLookup.TryGetValue(tag, out var p) ? p.prefab : null;
}

public interface IPoolable
{
    void OnCreated(string poolTag);
    void OnSpawn();
    void OnDespawn();
}