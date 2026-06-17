using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEntry
{
    public string name;
    public AudioClip clip;
    [Range(0f,1f)] public float volume = 1f;
    [Range(0.8f,1.2f)] public float pitchVariance = 0.05f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public List<SoundEntry> sounds = new();
    private AudioSource src;
    private Dictionary<string, SoundEntry> lookup = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        src = gameObject.AddComponent<AudioSource>();
        foreach (var s in sounds) lookup[s.name] = s;
    }

    public void Play(string name)
    {
        if (!lookup.TryGetValue(name, out var s)) return;
        float pitch = 1f + Random.Range(-s.pitchVariance, s.pitchVariance);
        src.pitch = pitch;
        src.PlayOneShot(s.clip, s.volume);
    }
}