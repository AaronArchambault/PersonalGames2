using UnityEngine;

/// <summary>
/// ParallaxBackground — looping parallax layer for ocean/sky backgrounds.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Range(0f, 1f)] public float parallaxFactor = 0.3f;
    public bool loopHorizontal = true;
    public bool loopVertical   = false;

    private Transform camTransform;
    private Vector3   lastCamPos;
    private float     spriteWidth;
    private float     spriteHeight;

    void Start()
    {
        camTransform = Camera.main.transform;
        lastCamPos   = camTransform.position;

        var sr = GetComponent<SpriteRenderer>();
        if (sr) { spriteWidth = sr.bounds.size.x; spriteHeight = sr.bounds.size.y; }
    }

    void LateUpdate()
    {
        Vector3 delta = camTransform.position - lastCamPos;
        lastCamPos    = camTransform.position;

        transform.position += new Vector3(delta.x * (1f - parallaxFactor),
                                          delta.y * (1f - parallaxFactor), 0f);

        if (loopHorizontal && spriteWidth > 0f)
        {
            float diffX = camTransform.position.x - transform.position.x;
            if (Mathf.Abs(diffX) >= spriteWidth)
                transform.position += new Vector3(Mathf.Sign(diffX) * spriteWidth, 0f, 0f);
        }

        if (loopVertical && spriteHeight > 0f)
        {
            float diffY = camTransform.position.y - transform.position.y;
            if (Mathf.Abs(diffY) >= spriteHeight)
                transform.position += new Vector3(0f, Mathf.Sign(diffY) * spriteHeight, 0f);
        }
    }
}