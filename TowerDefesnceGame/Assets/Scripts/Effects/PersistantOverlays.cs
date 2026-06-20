// Scripts/Effects/PersistentOverlays.cs
using UnityEngine;

public class PersistentOverlays : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}