using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
 
public enum WeatherType { None, Rain, Sun, Fog, Storm }
 
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }
 
    [Header("Weather")]
    public float weatherChangeDuration = 45f;  // seconds between weather changes
    public ParticleSystem rainParticles;
    public ParticleSystem sunRaysParticles;
    public Light2D        globalLight;
 
    public WeatherType CurrentWeather { get; private set; } = WeatherType.None;
 
    // Modifiers read by Enemy.cs and Tower.cs
    public float EnemySpeedModifier    { get; private set; } = 1f;
    public float TowerDamageModifier   { get; private set; } = 1f;
    public float TowerFireRateModifier { get; private set; } = 1f;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
 
    void Start()
    {
        StartCoroutine(WeatherCycle());
    }
 
    IEnumerator WeatherCycle()
    {
        yield return new WaitForSeconds(30f); // first weather after 30s
        while (true)
        {
            SetWeather((WeatherType)Random.Range(0, 5));
            yield return new WaitForSeconds(weatherChangeDuration);
            SetWeather(WeatherType.None);
            yield return new WaitForSeconds(15f);
        }
    }
 
    public void SetWeather(WeatherType type)
    {
        CurrentWeather = type;
 
        // Reset
        EnemySpeedModifier    = 1f;
        TowerDamageModifier   = 1f;
        TowerFireRateModifier = 1f;
 
        if (rainParticles)   rainParticles.Stop();
        if (sunRaysParticles) sunRaysParticles.Stop();
 
        switch (type)
        {
            case WeatherType.Rain:
                EnemySpeedModifier  = 0.7f;  // rain slows enemies
                TowerFireRateModifier = 0.9f; // towers a bit slower too
                if (rainParticles) rainParticles.Play();
                if (globalLight)   globalLight.color = new Color(0.7f, 0.8f, 1f);
                UIManager.Instance?.Announce("Rain! Enemies slow.", Color.cyan);
                break;
 
            case WeatherType.Sun:
                TowerDamageModifier   = 1.3f; // cats energised by sun
                TowerFireRateModifier = 1.1f;
                if (sunRaysParticles) sunRaysParticles.Play();
                if (globalLight) globalLight.color = new Color(1f, 0.98f, 0.8f);
                UIManager.Instance?.Announce("Sunny! Towers stronger.", Color.yellow);
                break;
 
            case WeatherType.Storm:
                EnemySpeedModifier  = 0.8f;
                TowerDamageModifier = 0.8f;  // hard to aim in a storm
                if (rainParticles) rainParticles.Play();
                if (globalLight) globalLight.color = new Color(0.5f, 0.5f, 0.7f);
                CameraShake.Instance?.Shake(0.05f, 0.5f);
                UIManager.Instance?.Announce("Storm! Everyone struggles.", Color.grey);
                break;
 
            case WeatherType.Fog:
                // Reduces tower range (applied in Tower.RecalculateStats)
                TowerDamageModifier = 0.9f;
                if (globalLight) globalLight.color = new Color(0.85f, 0.87f, 0.85f);
                UIManager.Instance?.Announce("Fog! Tower range reduced.", Color.grey);
                break;
 
            case WeatherType.None:
                if (globalLight) globalLight.color = Color.white;
                break;
        }
    }
}
 