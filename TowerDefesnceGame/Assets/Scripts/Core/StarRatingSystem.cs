
using UnityEngine;
using System.Collections.Generic;
 
public class StarRatingSystem : MonoBehaviour
{
    public static StarRatingSystem Instance { get; private set; }
 
    // levelName -> star count (1-3)
    private Dictionary<string, int> ratings = new();
 
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
        LoadRatings();
    }
 
    public void RegisterWave(int wave, int stars)
    {
        string key = $"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}_w{wave}";
        if (!ratings.ContainsKey(key) || ratings[key] < stars)
            ratings[key] = stars;
    }
 
    public int GetLevelStars(string levelName)
    {
        // Minimum stars across all waves = level star rating
        int min = 3;
        foreach (var kvp in ratings)
            if (kvp.Key.StartsWith(levelName) && kvp.Value < min)
                min = kvp.Value;
        return min;
    }
 
    public void SaveRatings()
    {
        foreach (var kvp in ratings)
            PlayerPrefs.SetInt($"stars_{kvp.Key}", kvp.Value);
        PlayerPrefs.Save();
    }
 
    void LoadRatings()
    {
        // PlayerPrefs doesn't have key enumeration so we'd need a saved key list
        // For now this is a basic implementation — expand with a JSON save if needed
    }
}
 