using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originPos;
    private Coroutine shakeRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        originPos = transform.localPosition;
    }

    public void Shake(float magnitude = 0.15f, float duration = 0.2f)
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(DoShake(magnitude, duration));
    }

    IEnumerator DoShake(float magnitude, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float strength = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            transform.localPosition = originPos + (Vector3)Random.insideUnitCircle * strength;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originPos;
    }
}