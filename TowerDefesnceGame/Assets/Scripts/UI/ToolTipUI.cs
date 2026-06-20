using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [Header("References")]
    public RectTransform tooltipPanel;
    public TextMeshProUGUI tooltipText;
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 8f;
    public Vector2 offset  = new Vector2(10f, -10f);
    public float padding   = 10f;

    private Canvas parentCanvas;
    private bool showing = false;
    private Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        parentCanvas = GetComponentInParent<Canvas>();
        if (canvasGroup == null) canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = tooltipPanel.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        tooltipPanel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!showing) return;

        // Follow the mouse
        Vector2 screenPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out screenPos);

        tooltipPanel.anchoredPosition = screenPos + offset;

        // Keep on screen
        ClampToScreen();
    }

    public void Show(string text, Vector3 worldPos)
    {
        tooltipText.text = text;
        tooltipPanel.gameObject.SetActive(true);
        showing = true;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        showing = false;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutAndDisable());
    }

    IEnumerator FadeTo(float target)
    {
        while (Mathf.Abs(canvasGroup.alpha - target) > 0.01f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, target,
                                           Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }
        canvasGroup.alpha = target;
    }

    IEnumerator FadeOutAndDisable()
    {
        yield return StartCoroutine(FadeTo(0f));
        tooltipPanel.gameObject.SetActive(false);
    }

    void ClampToScreen()
    {
        Canvas.ForceUpdateCanvases();
        Vector3[] corners = new Vector3[4];
        tooltipPanel.GetWorldCorners(corners);
        Rect canvasRect = (parentCanvas.transform as RectTransform).rect;

        float rightEdge  = corners[2].x;
        float bottomEdge = corners[0].y;
        Vector2 pos = tooltipPanel.anchoredPosition;

        if (rightEdge > canvasRect.width / 2f)
            pos.x -= rightEdge - canvasRect.width / 2f + padding;
        if (bottomEdge < -canvasRect.height / 2f)
            pos.y += (-canvasRect.height / 2f - bottomEdge) + padding;

        tooltipPanel.anchoredPosition = pos;
    }
}