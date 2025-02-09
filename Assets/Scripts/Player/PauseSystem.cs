using UnityEngine;
using UnityEngine.UI;

public class PauseSystem : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Button resumeButton, saveButton, loadButton, quitButton;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
        resumeButton.onClick.AddListener(ResumeGame);
        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void TogglePause()
    {
        bool isPaused = !pauseMenuUI.activeSelf;
        pauseMenuUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }

    private void ResumeGame()
    {
        TogglePause();
    }

    private void SaveGame()
    {
        Debug.Log("Save Game");
    }

    private void LoadGame()
    {
        Debug.Log("Load Game");
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
