/*using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class CowEntity : MonoBehaviour
{
    public CowData Data          { get; private set; }
    public GridCell CurrentCell  { get; set; }

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public TMP_Text       tierLabel;     // optional small tier number badge
    public TMP_Text       nameLabel;     // optional cow name under sprite

    public void Initialize(CowData data, GridCell cell)
    {
        Data        = data;
        CurrentCell = cell;

        // Apply visuals
        spriteRenderer.sprite = data.sprite;
        spriteRenderer.color  = data.tintColor;

        if (tierLabel) tierLabel.text = data.tier.ToString();
        if (nameLabel) nameLabel.text = data.cowName;

        // Animate in with a little pop scale
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.25f).setEaseOutBack();
    }

    // Call this from DragHandler when a drag starts
    public void OnPickUp()
    {
        LeanTween.scale(gameObject, Vector3.one * 1.15f, 0.1f).setEaseOutQuad();
        GetComponent<SpriteRenderer>().sortingOrder = 10;
    }

    // Call when dropped (not merged)
    public void OnPutDown(GridCell newCell)
    {
        CurrentCell           = newCell;
        transform.position    = newCell.worldPosition;
        LeanTween.scale(gameObject, Vector3.one, 0.1f).setEaseOutQuad();
        GetComponent<SpriteRenderer>().sortingOrder = 1;
    }
}
*/


using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class CowEntity : MonoBehaviour
{
    public CowData Data          { get; private set; }
    public GridCell CurrentCell  { get; set; }

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public TMP_Text       tierLabel;
    public TMP_Text       nameLabel;

    public void Initialize(CowData data, GridCell cell)
    {
        Data        = data;
        CurrentCell = cell;

        spriteRenderer.sprite = data.sprite;
        spriteRenderer.color  = data.tintColor;

        if (tierLabel) tierLabel.text = data.tier.ToString();
        if (nameLabel) nameLabel.text = data.cowName;

        // Pop-in animation (replaces LeanTween)
        transform.localScale = Vector3.zero;
        StartCoroutine(ScaleTo(Vector3.one, 0.25f, overshoot: true));
    }

    public void OnPickUp()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(Vector3.one * 1.15f, 0.1f));
        GetComponent<SpriteRenderer>().sortingOrder = 10;
    }

    public void OnPutDown(GridCell newCell)
    {
        CurrentCell        = newCell;
        transform.position = newCell.worldPosition;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(Vector3.one, 0.1f));
        GetComponent<SpriteRenderer>().sortingOrder = 1;
    }

    // Simple scale coroutine — overshoot=true gives a bouncy pop-in feel
    IEnumerator ScaleTo(Vector3 target, float duration, bool overshoot = false)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = overshoot ? EaseOutBack(t) : EaseOutQuad(t);
            transform.localScale = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }
        transform.localScale = target;
    }

    float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}