// ─────────────────────────────────────────────────────────────────────────────
// CameraFollow.cs
// ─────────────────────────────────────────────────────────────────────────────
// Save as: Assets/Scripts/CameraFollow.cs

using UnityEngine;

/// <summary>Smoothly follows the player and zooms out as they grow.</summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed    = 5f;
    public Vector3 offset       = new Vector3(0f, 0f, -10f);
    public float baseOrthoSize  = 6f;
    public float zoomMultiplier = 0.9f;
    public float maxOrthoSize   = 60f;
    public float zoomSmoothSpeed = 3f;

    private Camera cam;
    private PlayerCreature player;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = baseOrthoSize;
    }

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
        player = target?.GetComponent<PlayerCreature>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = Vector3.Lerp(transform.position,
            target.position + offset, smoothSpeed * Time.deltaTime);

        if (player != null)
        {
            float targetSize = Mathf.Clamp(
                baseOrthoSize * player.CurrentSize * zoomMultiplier,
                baseOrthoSize, maxOrthoSize);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize,
                targetSize, zoomSmoothSpeed * Time.deltaTime);
        }
    }
}