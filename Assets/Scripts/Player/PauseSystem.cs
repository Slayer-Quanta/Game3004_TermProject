using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseSystem : MonoBehaviour
{
    public static PauseSystem self;

    public Image healthBar;
    public GameObject healthBarCanvas;
    public GameObject pauseMenuUI;
    public Button resumeButton, saveButton, loadButton, quitButton;
    private GameObject player;

    private void Awake()
    {
        self = this;
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");

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
            healthBarCanvas.SetActive(!isPaused);

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            SaveGame();  
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
    public void SaveGame()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            SaveSystem.SaveGame(player.transform.position);
        }
        else
        {
            Debug.LogWarning("Player not found. Cannot save the game.");
        }
    }

    public void LoadGame()
    {
        if (SaveSystem.ShouldLoadGame())  
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("No saved game found.");
        }
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
