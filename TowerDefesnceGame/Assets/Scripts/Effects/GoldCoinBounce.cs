using UnityEngine;
using System.Collections;

public class GoldCoinBounce : MonoBehaviour, IPoolable
{
    public string poolTag = "GoldCoin";
    public float  arcHeight  = 1.5f;
    public float  travelTime = 0.6f;
    public SpriteRenderer coinSprite;

    private Vector3 startPos;
    private Vector3 endPos;
    private bool    done;

    public void OnCreated(string tag) => poolTag = tag;
    public void OnDespawn() => done = false;
    public void OnSpawn()
    {
        done = false;
        if (coinSprite) coinSprite.enabled = true;
    }

    public void Launch(Vector3 from, Vector3 to)
    {
        startPos = from;
        endPos   = to;
        transform.position = from;
        StartCoroutine(ArcToHUD());
    }

    IEnumerator ArcToHUD()
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / travelTime;
            // Parabolic arc
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = pos;

            // Spin the coin
            transform.Rotate(0, 0, 720f * Time.deltaTime);

            // Scale down as it arrives
            float scale = Mathf.Lerp(1f, 0.2f, t);
            transform.localScale = Vector3.one * scale;

            yield return null;
        }
        done = true;
        ObjectPool.Instance.Despawn(poolTag, gameObject);
    }
}