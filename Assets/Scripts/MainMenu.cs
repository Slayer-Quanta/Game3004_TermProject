using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject worldCreationCanvas;

    private string savePath => Application.persistentDataPath + "/savegame.json";

    private void Start()
    {
        worldCreationCanvas.SetActive(false); 
    }

    public void PlayGame()
    {
        SaveSystem.DeleteSave(); 
        AudioManager.instance.PlayButtonClick();
        SceneManager.LoadSceneAsync(4); 
    }

    public void LoadGame()
    {
        if (SaveSystem.ShouldLoadGame())
        {
            AudioManager.instance.PlayButtonClick();
            SceneManager.LoadSceneAsync(4);
        }
        else
        {
            Debug.Log("No saved game found.");
        }
    }

    public void ToggleWorldCreationCanvas()
    {
        bool isActive = worldCreationCanvas.activeSelf;
        worldCreationCanvas.SetActive(!isActive);
        mainMenuCanvas.SetActive(isActive);
    }

    public void Options()
    {
        AudioManager.instance.PlayButtonClick();
        SceneManager.LoadSceneAsync(2);
    }

    public void Instructions()
    {
        AudioManager.instance.PlayButtonClick();
        SceneManager.LoadSceneAsync(1);
    }

    public void Achievements()
    {
        AudioManager.instance.PlayButtonClick();
        SceneManager.LoadSceneAsync(3);
    }

    public void Home()
    {
        AudioManager.instance.PlayButtonClick();
        SceneManager.LoadSceneAsync(0);
    }

    public void Exit()
    {
        AudioManager.instance.PlayButtonClick();
        Application.Quit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
        Debug.Log("Game is exiting...");
    }

    public void ShowSettingsPanel()
    {
        AudioManager.instance.PlayButtonClick();
        optionsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ShowOptionsPanel()
    {
        AudioManager.instance.PlayButtonClick();
        settingsPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void KeyboardOptions()
    {
        AudioManager.instance.PlayButtonClick();
        SceneManager.LoadSceneAsync(5);
    }
}
