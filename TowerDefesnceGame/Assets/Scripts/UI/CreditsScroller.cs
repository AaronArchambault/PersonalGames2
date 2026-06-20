using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    public CreditsData creditsData;
    public Transform   contentParent;  // Vertical Layout Group object
    public GameObject  sectionHeaderPrefab;
    public GameObject  creditEntryPrefab;
    public float       scrollSpeed = 40f;
    public Button      skipButton;

    private RectTransform contentRect;
    private bool scrolling = false;

    void OnEnable()
    {
        BuildCredits();
        StartCoroutine(ScrollCredits());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        scrolling = false;
    }

    void BuildCredits()
    {
        // Clear existing
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (creditsData == null) return;

        foreach (var section in creditsData.sections)
        {
            // Section header
            if (sectionHeaderPrefab)
            {
                var header = Instantiate(sectionHeaderPrefab, contentParent);
                var tmp = header.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp) tmp.text = section.sectionTitle;
            }

            // Entries
            foreach (var entry in section.entries)
            {
                if (creditEntryPrefab)
                {
                    var row = Instantiate(creditEntryPrefab, contentParent);
                    var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
                    if (texts.Length >= 2)
                    {
                        texts[0].text = entry.role;
                        texts[1].text = entry.name;
                    }
                }
            }
        }

        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
        contentRect = contentParent as RectTransform;
    }

    IEnumerator ScrollCredits()
    {
        yield return new WaitForSeconds(1f);
        scrolling = true;

        // Start below the panel, scroll upward
        Vector2 startPos = contentRect.anchoredPosition;
        float totalHeight = contentRect.rect.height;
        float target = startPos.y + totalHeight + 200f;

        while (scrolling && contentRect.anchoredPosition.y < target)
        {
            contentRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }
        scrolling = false;
    }

    public void OnSkip() => scrolling = false;
}