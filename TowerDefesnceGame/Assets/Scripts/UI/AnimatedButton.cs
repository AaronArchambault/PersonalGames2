using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class AnimatedButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    IPointerClickHandler
{
    // ── Hover settings ────────────────────────────────
    [Header("Hover")]
    public float hoverScale      = 1.1f;
    public float hoverScaleSpeed = 8f;
    public Color hoverColor      = new Color(1f, 1f, 0.8f, 1f); // warm white
    public bool  liftOnHover     = true;   // moves up slightly
    public float liftAmount      = 4f;     // pixels to lift

    // ── Press settings ────────────────────────────────
    [Header("Press")]
    public float pressScale      = 0.93f;
    public Color pressColor      = new Color(0.7f, 0.9f, 1f, 1f); // light blue
    public float pressScaleSpeed = 20f;

    // ── Colors ────────────────────────────────────────
    [Header("Colors")]
    public Color normalColor     = Color.white;
    public Color disabledColor   = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    // ── Sound ─────────────────────────────────────────
    [Header("Sound")]
    public string hoverSoundName = "btn_hover";
    public string clickSoundName = "btn_click";
    public string denySoundName  = "btn_deny";

    // ── Tooltip ───────────────────────────────────────
    [Header("Tooltip")]
    public string  tooltipText   = "";
    public float   tooltipDelay  = 0.6f;

    // ── Pulse (for important buttons) ─────────────────
    [Header("Pulse")]
    public bool  pulseWhenIdle   = false;
    public float pulseScale      = 1.05f;
    public float pulseSpeed      = 1.5f;

    // ── Shake (for disabled/deny) ─────────────────────
    [Header("Shake")]
    public float shakeAmount     = 6f;
    public float shakeDuration   = 0.4f;

    // ── References ────────────────────────────────────
    [Header("References")]
    public Image        buttonImage;  // the button background image
    public TextMeshProUGUI buttonText; // optional — text inside button

    // ── Runtime ───────────────────────────────────────
    private RectTransform rt;
    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Color   originalImageColor;
    private Color   originalTextColor;

    private bool isHovered  = false;
    private bool isPressed  = false;
    private bool isDisabled = false;

    private Coroutine scaleRoutine;
    private Coroutine colorRoutine;
    private Coroutine pulseRoutine;
    private Coroutine tooltipRoutine;
    private Coroutine liftRoutine;
    private Coroutine shakeRoutine;

    // Target values for smooth lerp
    private float targetScale = 1f;
    private Color targetColor;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        originalScale    = rt.localScale;
        originalPosition = rt.anchoredPosition;

        if (buttonImage == null) buttonImage = GetComponent<Image>();
        if (buttonText  == null) buttonText  = GetComponentInChildren<TextMeshProUGUI>();

        if (buttonImage) originalImageColor = buttonImage.color;
        if (buttonText)  originalTextColor  = buttonText.color;

        targetColor = normalColor;
    }

    void Start()
    {
        if (pulseWhenIdle)
            pulseRoutine = StartCoroutine(PulseLoop());
    }

    void Update()
    {
        if (isDisabled || shakeRoutine != null) return;

        // Smooth scale
        float target = isPressed ? pressScale
                     : isHovered ? hoverScale
                     : 1f;
        float speed  = isPressed ? pressScaleSpeed : hoverScaleSpeed;
        float currentScale = rt.localScale.x / originalScale.x;
        float newScale = Mathf.Lerp(currentScale, target, Time.unscaledDeltaTime * speed);
        rt.localScale = originalScale * newScale;

        // Smooth color
        Color targetCol = isPressed ? pressColor
                        : isHovered ? hoverColor
                        : normalColor;
        if (buttonImage)
            buttonImage.color = Color.Lerp(buttonImage.color, targetCol,
                                           Time.unscaledDeltaTime * hoverScaleSpeed);
    }

    // ── Pointer Events ────────────────────────────────

    public void OnPointerEnter(PointerEventData e)
    {
        if (isDisabled) return;
        isHovered = true;

        // Stop pulse while hovered
        if (pulseRoutine != null) { StopCoroutine(pulseRoutine); pulseRoutine = null; }

        // Lift up
        if (liftOnHover)
        {
            if (liftRoutine != null) StopCoroutine(liftRoutine);
            liftRoutine = StartCoroutine(LiftTo(originalPosition + Vector2.up * liftAmount));
        }

        // Sound
        AudioManager.Instance?.Play(hoverSoundName);

        // Tooltip
        if (!string.IsNullOrEmpty(tooltipText))
        {
            if (tooltipRoutine != null) StopCoroutine(tooltipRoutine);
            tooltipRoutine = StartCoroutine(ShowTooltipDelayed());
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (isDisabled) return;
        isHovered = false;
        isPressed = false;

        // Lower back
        if (liftOnHover)
        {
            if (liftRoutine != null) StopCoroutine(liftRoutine);
            liftRoutine = StartCoroutine(LiftTo(originalPosition));
        }

        // Hide tooltip
        if (tooltipRoutine != null) { StopCoroutine(tooltipRoutine); tooltipRoutine = null; }
        TooltipUI.Instance?.Hide();

        // Restart pulse
        if (pulseWhenIdle)
            pulseRoutine = StartCoroutine(PulseLoop());
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (isDisabled) return;
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (isDisabled) return;
        isPressed = false;
        // Spring bounce back
        StartCoroutine(SpringBounce());
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (isDisabled)
        {
            // Shake to show it's disabled
            PlayDenyShake();
            AudioManager.Instance?.Play(denySoundName);
            return;
        }
        AudioManager.Instance?.Play(clickSoundName);
        StartCoroutine(ClickFlash());
    }

    // ── Public API ────────────────────────────────────

    public void SetDisabled(bool disabled)
    {
        isDisabled = disabled;
        if (buttonImage)
            buttonImage.color = disabled ? disabledColor : normalColor;
        if (buttonText)
            buttonText.color = disabled
                ? new Color(originalTextColor.r, originalTextColor.g,
                            originalTextColor.b, 0.5f)
                : originalTextColor;
    }

    public void PlayDenyShake()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(Shake());
    }

    public void TriggerPulseOnce()
    {
        StartCoroutine(PulseOnce());
    }

    // ── Coroutines ────────────────────────────────────

    IEnumerator SpringBounce()
    {
        // Overshoot slightly past normal then settle
        float t = 0;
        while (t < 0.3f)
        {
            float overshoot = 1f + Mathf.Sin(t / 0.3f * Mathf.PI) * 0.06f;
            rt.localScale = originalScale * overshoot;
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        rt.localScale = originalScale;
    }

    IEnumerator ClickFlash()
    {
        // Quick white flash
        if (buttonImage)
        {
            Color flash = Color.white;
            buttonImage.color = flash;
            yield return new WaitForSecondsRealtime(0.05f);
            buttonImage.color = normalColor;
        }
    }

    IEnumerator PulseLoop()
    {
        while (true)
        {
            float t = 0;
            while (t < 1f)
            {
                float s = 1f + Mathf.Sin(t * Mathf.PI * 2f * pulseSpeed)
                              * (pulseScale - 1f) * 0.5f;
                rt.localScale = originalScale * s;
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    IEnumerator PulseOnce()
    {
        float t = 0;
        while (t < 0.4f)
        {
            float s = 1f + Mathf.Sin(t / 0.4f * Mathf.PI) * (pulseScale - 1f);
            rt.localScale = originalScale * s;
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        rt.localScale = originalScale;
    }

    IEnumerator Shake()
    {
        Vector3 startPos = rt.localPosition;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float strength = Mathf.Lerp(shakeAmount, 0f, elapsed / shakeDuration);
            rt.localPosition = startPos + (Vector3)Random.insideUnitCircle * strength;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rt.localPosition = startPos;
        shakeRoutine = null;
    }

    IEnumerator LiftTo(Vector2 target)
    {
        float t = 0;
        Vector2 start = rt.anchoredPosition;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * hoverScaleSpeed;
            rt.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }
        rt.anchoredPosition = target;
    }

    IEnumerator ShowTooltipDelayed()
    {
        yield return new WaitForSecondsRealtime(tooltipDelay);
        TooltipUI.Instance?.Show(tooltipText, transform.position);
    }
}