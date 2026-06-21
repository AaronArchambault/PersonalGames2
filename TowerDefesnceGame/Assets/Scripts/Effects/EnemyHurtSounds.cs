using UnityEngine;

public class EnemyHurtSounds : MonoBehaviour
{
    public string[] hurtSoundNames;  // e.g. "mouse_squeak_1", "mouse_squeak_2"
    public string   deathSoundName;
    public float    soundCooldown = 0.3f;

    private float lastSoundTime = -1f;
    private Enemy enemy;

    void Awake() => enemy = GetComponent<Enemy>();

    void OnEnable()
    {
        // Hook into the enemy's damage via a simple check
        // Since Enemy doesn't have an OnDamaged event we use Update
        lastSoundTime = -1f;
    }

    // Call this from Enemy.TakeDamage() — add one line there:
    // GetComponent<EnemyHurtSounds>()?.PlayHurtSound();
    public void PlayHurtSound()
    {
        if (Time.time - lastSoundTime < soundCooldown) return;
        lastSoundTime = Time.time;

        if (hurtSoundNames == null || hurtSoundNames.Length == 0) return;
        string sound = hurtSoundNames[Random.Range(0, hurtSoundNames.Length)];
        AudioManager.Instance?.Play(sound);
    }

    public void PlayDeathSound()
    {
        if (!string.IsNullOrEmpty(deathSoundName))
            AudioManager.Instance?.Play(deathSoundName);
    }
}