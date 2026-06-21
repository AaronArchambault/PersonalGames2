using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
 
[System.Serializable]
public class SoundEntry
{
    public string    name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.8f, 1.2f)] public float pitchVariance = 0.05f;
}
 
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
 
    public List<SoundEntry> sounds = new();
 
    [Header("Mixer")]
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup bassMixerGroup;  // separate group for bombs/explosions
 
    private AudioSource src;
    private AudioSource bassSource;
    private Dictionary<string, SoundEntry> lookup = new();
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
 
        src        = gameObject.AddComponent<AudioSource>();
        bassSource = gameObject.AddComponent<AudioSource>();
 
        src.outputAudioMixerGroup        = sfxMixerGroup;
        if (bassMixerGroup != null)
            bassSource.outputAudioMixerGroup = bassMixerGroup;
 
        foreach (var s in sounds) lookup[s.name] = s;
    }
 
    public void Play(string name)
    {
        if (!lookup.TryGetValue(name, out var s) || s.clip == null) return;
        src.pitch = 1f + Random.Range(-s.pitchVariance, s.pitchVariance);
        src.PlayOneShot(s.clip, s.volume);
    }
 
    // Play with explicit pitch — used for damage-scaled hit sounds
    public void PlayWithPitch(string name, float pitch)
    {
        if (!lookup.TryGetValue(name, out var s) || s.clip == null) return;
        src.pitch = pitch;
        src.PlayOneShot(s.clip, s.volume);
    }
 
    // Play through the bass channel — lower frequencies for bombs
    public void PlayBass(string name)
    {
        if (!lookup.TryGetValue(name, out var s) || s.clip == null) return;
        bassSource.pitch = 1f;
        bassSource.PlayOneShot(s.clip, s.volume);
    }
}