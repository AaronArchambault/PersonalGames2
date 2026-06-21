
using UnityEngine;
using UnityEngine.Rendering.Universal;
 
public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }
 
    public Light2D globalLight;
    public float   cycleDuration = 120f;  // seconds per full day/night cycle
 
    [Header("Colors")]
    public Color dawn    = new Color(1f, 0.8f, 0.6f, 1f);
    public Color midday  = new Color(1f, 1f, 0.95f, 1f);
    public Color dusk    = new Color(1f, 0.6f, 0.4f, 1f);
    public Color night   = new Color(0.3f, 0.3f, 0.6f, 1f);
 
    private float timer = 0f;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
 
    void Update()
    {
        // Don't run if weather is overriding the light
        if (WeatherSystem.Instance?.CurrentWeather != WeatherType.None) return;
 
        timer += Time.deltaTime;
        float t = (timer % cycleDuration) / cycleDuration; // 0-1
 
        Color lightColor;
        float intensity;
 
        if (t < 0.25f)       { lightColor = Color.Lerp(night, dawn,   t / 0.25f);   intensity = Mathf.Lerp(0.4f, 0.8f, t / 0.25f); }
        else if (t < 0.5f)  { lightColor = Color.Lerp(dawn,  midday, (t-0.25f)/0.25f); intensity = Mathf.Lerp(0.8f, 1f,  (t-0.25f)/0.25f); }
        else if (t < 0.75f) { lightColor = Color.Lerp(midday, dusk,  (t-0.5f)/0.25f);  intensity = Mathf.Lerp(1f,  0.7f, (t-0.5f)/0.25f); }
        else                  { lightColor = Color.Lerp(dusk,  night, (t-0.75f)/0.25f); intensity = Mathf.Lerp(0.7f, 0.4f, (t-0.75f)/0.25f); }
 
        if (globalLight)
        {
            globalLight.color     = lightColor;
            globalLight.intensity = intensity;
        }
    }
 
    public bool IsNight => (timer % cycleDuration) / cycleDuration > 0.75f;
}