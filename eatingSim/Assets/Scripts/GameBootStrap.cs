using UnityEngine;

/// <summary>
/// GameBootstrap — reads PlayerPrefs set by CharacterSelectManager
/// and applies the chosen character stats to the PlayerCreature.
/// Place one instance in the Game scene.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        var player = FindObjectOfType<PlayerCreature>();
        if (player == null) return;

        if (PlayerPrefs.HasKey("CharStartSize"))
        {
            player.startSize         = PlayerPrefs.GetFloat("CharStartSize",     0.4f);
            player.baseSpeed         = PlayerPrefs.GetFloat("CharBaseSpeed",     5f);
            player.massPerSizeDouble = PlayerPrefs.GetFloat("CharMassPerDouble", 50f);
            player.transform.localScale = Vector3.one * player.startSize;
        }

        string name = PlayerPrefs.GetString("CharName", "Goldfish");
        Debug.Log($"[GameBootstrap] Playing as: {name}");
    }
}