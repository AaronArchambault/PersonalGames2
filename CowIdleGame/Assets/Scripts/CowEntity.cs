using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class CowEntity : MonoBehaviour
{
    public CowData  Data        { get; private set; }
    public GridCell CurrentCell { get; set; }
    public bool     IsDragging  { get; private set; }
    public bool     IsMoving    { get; private set; }

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public TMP_Text       tierLabel;
    public TMP_Text       nameLabel;

    [Header("Wander Settings")]
    [Tooltip("Seconds between random moves (randomised per cow so they don't all move at once)")]
    public float wanderIntervalMin = 4f;
    public float wanderIntervalMax = 9f;

    Coroutine _wanderCoroutine;
    Coroutine _moveCoroutine;

    // ── Init ──────────────────────────────────────────────────────────────────
    public void Initialize(CowData data, GridCell cell)
    {
        Data        = data;
        CurrentCell = cell;

        spriteRenderer.sprite = data.sprite;
        spriteRenderer.color  = data.tintColor;

        if (tierLabel) tierLabel.text = data.tier.ToString();
        if (nameLabel) nameLabel.text = data.cowName;

        transform.localScale = Vector3.zero;
        StartCoroutine(ScaleTo(Vector3.one, 0.25f, overshoot: true));

        _wanderCoroutine = StartCoroutine(WanderRoutine());
    }

    // ── Drag callbacks (called by DragHandler) ────────────────────────────────
    public void OnPickUp()
    {
        IsDragging = true;
        StopWander();
        StopMove();
        StartCoroutine(ScaleTo(Vector3.one * 1.2f, 0.12f));
        spriteRenderer.sortingOrder = 10;
    }

    // Smooth slide to a cell (drag drop or click-to-move)
    public void OnPutDown(GridCell newCell, bool instant = false)
    {
        IsDragging       = false;
        CurrentCell      = newCell;
        spriteRenderer.sortingOrder = 1;
        StartCoroutine(ScaleTo(Vector3.one, 0.12f));

        if (instant)
            transform.position = newCell.worldPosition;
        else
            StartMove(newCell.worldPosition);

        _wanderCoroutine = StartCoroutine(WanderRoutine());
    }

    // Called when a click-to-move is requested (not a drag)
    public void SlideToCell(GridCell newCell)
    {
        if (IsMoving || IsDragging) return;
        CurrentCell = newCell;
        StartMove(newCell.worldPosition);
    }

    // ── Movement ──────────────────────────────────────────────────────────────
    void StartMove(Vector3 destination)
    {
        StopMove();
        _moveCoroutine = StartCoroutine(SlideTo(destination, 0.22f));
    }

    void StopMove()
    {
        if (_moveCoroutine != null) { StopCoroutine(_moveCoroutine); _moveCoroutine = null; }
        IsMoving = false;
    }

    IEnumerator SlideTo(Vector3 destination, float duration)
    {
        IsMoving = true;
        Vector3 start   = transform.position;
        float   elapsed = 0f;

        // Tiny squash on departure
        StartCoroutine(ScaleTo(new Vector3(0.85f, 1.15f, 1f), 0.06f));

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.LerpUnclamped(start, destination, t);
            yield return null;
        }

        transform.position = destination;
        IsMoving           = false;

        // Tiny squash on landing
        StartCoroutine(ScaleTo(new Vector3(1.15f, 0.85f, 1f), 0.05f));
        yield return new WaitForSeconds(0.05f);
        StartCoroutine(ScaleTo(Vector3.one, 0.1f, overshoot: true));
    }

    // ── Wander ────────────────────────────────────────────────────────────────
    void StopWander()
    {
        if (_wanderCoroutine != null) { StopCoroutine(_wanderCoroutine); _wanderCoroutine = null; }
    }

    IEnumerator WanderRoutine()
    {
        // Stagger start so all cows don't move at the same frame
        yield return new WaitForSeconds(Random.Range(wanderIntervalMin, wanderIntervalMax));

        while (true)
        {
            if (!IsDragging && !IsMoving)
            {
                GridCell target = GridManager.Instance.GetRandomEmptyCell();
                if (target != null)
                {
                    // Vacate current cell, occupy target
                    CurrentCell.Occupant = null;
                    target.Occupant      = this;
                    SlideToCell(target);
                }
            }
            yield return new WaitForSeconds(Random.Range(wanderIntervalMin, wanderIntervalMax));
        }
    }

    // ── Scale animation ───────────────────────────────────────────────────────
    Coroutine _scaleCoroutine;

    IEnumerator ScaleTo(Vector3 target, float duration, bool overshoot = false)
    {
        Vector3 start   = transform.localScale;
        float   elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t     = Mathf.Clamp01(elapsed / duration);
            float eased = overshoot ? EaseOutBack(t) : EaseOutQuad(t);
            transform.localScale = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }
        transform.localScale = target;
    }

    // ── Easing ────────────────────────────────────────────────────────────────
    float EaseOutQuad(float t)  => 1f - (1f - t) * (1f - t);
    float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}






/*using System.Collections;
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
}*/


