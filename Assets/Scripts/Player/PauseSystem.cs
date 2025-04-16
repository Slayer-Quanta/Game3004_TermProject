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
    private World world;

    private void Awake()
    {
        self = this;
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        world = FindObjectOfType<World>();

        pauseMenuUI.SetActive(false);
        resumeButton.onClick.AddListener(ResumeGame);
        saveButton.onClick.AddListener(SaveGame);
        //loadButton.onClick.AddListener(LoadGame);
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

    //public void LoadGame()
    //{
    //    if (SaveSystem.ShouldLoadGame())
    //    {
    //        Debug.Log("Loading game...");

    //        // First resume the game to set timeScale to 1
    //        ResumeGame();

    //        // Then reload the scene
    //        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    //    }
    //    else
    //    {
    //        Debug.Log("No saved game found.");
    //    }
    //}

    // Ensure ResumeGame properly resets state
    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenuUI.SetActive(false);
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update SaveGame to include proper error checking
    public void SaveGame()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player");

        if (world == null)
            world = FindObjectOfType<World>();

        if (player != null && world != null)
        {
            Debug.Log("Saving game at position: " + player.transform.position);
            SaveSystem.SaveGame(player.transform.position, world);
        }
        else
        {
            Debug.LogWarning("Player or World not found. Cannot save the game.");
        }
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}