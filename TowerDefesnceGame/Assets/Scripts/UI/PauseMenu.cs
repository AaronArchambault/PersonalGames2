using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsPanel;
    private bool isPaused = false;

    void OnEnable()
    {
        InputHandler.Instance.OnCancelPerformed += TogglePause;
    }

    void OnDisable()
    {
        if (InputHandler.Instance)
            InputHandler.Instance.OnCancelPerformed -= TogglePause;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void OnResume()      => TogglePause();
    public void OnSettings()    => settingsPanel.SetActive(true);
    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    public void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}