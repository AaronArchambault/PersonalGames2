using UnityEngine;
using System.Collections;

public class TowerIdleAnimator : MonoBehaviour
{
    [Header("Sleeping")]
    public ParticleSystem zzzParticles;
    public float          sleepCheckInterval = 1f;

    [Header("Happy Dance")]
    public float danceHeight    = 0.15f;
    public float danceDuration  = 1.5f;
    public float danceSpeed     = 8f;

    [Header("Tail Wag")]
    public Transform tailTransform;  // child transform for the tail sprite
    public float     wagSpeed  = 3f;
    public float     wagAmount = 15f; // degrees

    private Tower    tower;
    private bool     isSleeping = false;
    private Vector3  startPos;

    void Start()
    {
        tower    = GetComponent<Tower>();
        startPos = transform.position;

        StartCoroutine(SleepCheck());
        StartCoroutine(TailWag());

        // Subscribe to wave complete
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete += OnWaveComplete;
    }

    void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= OnWaveComplete;
    }

    IEnumerator SleepCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(sleepCheckInterval);
            bool enemiesNear = Physics2D.OverlapCircle(
                transform.position, tower != null ? tower.baseRange : 3f,
                LayerMask.GetMask("Enemy"));

            if (!enemiesNear && !isSleeping)  StartSleep();
            else if (enemiesNear && isSleeping) StopSleep();
        }
    }

    void StartSleep()
    {
        isSleeping = true;
        zzzParticles?.Play();
    }

    void StopSleep()
    {
        isSleeping = false;
        zzzParticles?.Stop();
    }

    void OnWaveComplete()
    {
        StartCoroutine(HappyDance());
    }

    IEnumerator HappyDance()
    {
        float t = 0;
        while (t < danceDuration)
        {
            float y = Mathf.Sin(t * danceSpeed * Mathf.PI * 2f) * danceHeight;
            transform.position = startPos + Vector3.up * y;
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = startPos;
    }

    IEnumerator TailWag()
    {
        if (tailTransform == null) yield break;
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime * wagSpeed;
            float angle = Mathf.Sin(timer) * wagAmount;
            tailTransform.localRotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }
    }
}