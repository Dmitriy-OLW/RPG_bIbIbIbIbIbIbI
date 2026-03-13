using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;
    private int sceneIndex;
    private void Start()
    {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
    }
    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isPaused)
            Resume();
        else
            Pause();
    }
    private void Pause()
    {
        pausePanel.SetActive(true);
        hudPanel.SetActive(false);

        Time.timeScale = 0f;

        isPaused = true;
    }

    private void Resume()
    {
        pausePanel.SetActive(false);
        hudPanel.SetActive(true);

        Time.timeScale = 1f;

        isPaused = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }

    public void BackToMenu(int index)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(index);
    }
    public void NewGame()
    {
        SceneManager.LoadScene(1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
