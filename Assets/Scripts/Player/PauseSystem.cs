using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Add this for UI input handling

public class PauseSystem : MonoBehaviour
{
    public Image healthBar;
    public GameObject healthBarCanvas;
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

        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }

         
        

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

         
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    

    public void ResumeGame()
    {
        Time.timeScale = 1;  
        pauseMenuUI.SetActive(false);
        healthBarCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
