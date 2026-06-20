using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject audioPanel;
    public GameObject accessibilityPanel;
    public GameObject creditsPanel;
    public GameObject confirmQuitPanel;

    [Header("Main Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Transition")]
    public CanvasGroup fadeOverlay;
    public float fadeTime = 0.5f;

    [Header("Version")]
    public TextMeshProUGUI versionText;

    void Start()
    {
        // Show main panel, hide all others
        ShowPanel(mainPanel);

        if (versionText)
            versionText.text = $"v{Application.version}";

        // Fade in from black
        StartCoroutine(FadeIn());
    }

    // ── Panel Navigation ──────────────────────────────

    void ShowPanel(GameObject target)
    {
        mainPanel.SetActive(target == mainPanel);
        settingsPanel.SetActive(target == settingsPanel);
        if (audioPanel)       audioPanel.SetActive(target == audioPanel);
        if (accessibilityPanel) accessibilityPanel.SetActive(target == accessibilityPanel);
        if (creditsPanel)     creditsPanel.SetActive(target == creditsPanel);
        if (confirmQuitPanel) confirmQuitPanel.SetActive(target == confirmQuitPanel);
    }

    // ── Button Callbacks ──────────────────────────────

    public void OnPlay()
    {
        StartCoroutine(FadeOutAndLoad("SampleScene"));
    }

    public void OnSettings()    => ShowPanel(settingsPanel);
    public void OnAudio()       => ShowPanel(audioPanel);
    public void OnAccessibility() => ShowPanel(accessibilityPanel);
    public void OnCredits()     => ShowPanel(creditsPanel);

    public void OnBack()        => ShowPanel(mainPanel);

    public void OnQuit()        => ShowPanel(confirmQuitPanel);
    public void OnConfirmQuit() => Application.Quit();
    public void OnCancelQuit()  => ShowPanel(mainPanel);

    // ── Transitions ───────────────────────────────────

    IEnumerator FadeIn()
    {
        if (fadeOverlay == null) yield break;
        fadeOverlay.alpha = 1f;
        float t = 0;
        while (t < fadeTime)
        {
            fadeOverlay.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }
        fadeOverlay.alpha = 0f;
    }

    IEnumerator FadeOutAndLoad(string scene)
    {
        if (fadeOverlay != null)
        {
            float t = 0;
            while (t < fadeTime)
            {
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
                t += Time.deltaTime;
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }
        SceneManager.LoadScene(scene);
    }
}