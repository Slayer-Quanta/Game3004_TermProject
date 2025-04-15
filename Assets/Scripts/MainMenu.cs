using System.Collections;
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
        SaveSystem.DeleteSave();  // Clear any previous save file to start fresh
        AudioManager.instance.PlayButtonClick();

        // Show loading screen before loading the scene
        LoadingScreen.Instance.ShowLoadingScreen();

        // Load the scene asynchronously
        StartCoroutine(LoadGameSceneAsync(4));
    }

    private IEnumerator LoadGameSceneAsync(int sceneIndex)
    {
        // Begin loading the scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        // Don't let the scene activate until we allow it
        operation.allowSceneActivation = false;

        // While the scene loads
        while (!operation.isDone)
        {
            // Update the progress bar (normalized progress goes from 0 to 0.9)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            LoadingScreen.Instance.UpdateProgress(progress);

            // If the load has finished
            if (operation.progress >= 0.9f)
            {
                // Wait for a short delay to show the last loading message
                yield return new WaitForSeconds(1f);

                // Activate the scene
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Hide loading screen when done (though likely won't be seen as we're changing scenes)
        LoadingScreen.Instance.HideLoadingScreen();
    }

    public void LoadGame()
    {
        if (SaveSystem.ShouldLoadGame())
        {
            AudioManager.instance.PlayButtonClick();

            // Show loading screen before loading the scene
            LoadingScreen.Instance.ShowLoadingScreen();

            // Load the scene asynchronously
            StartCoroutine(LoadGameSceneAsync(4));
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
        SceneManager.LoadSceneAsync(2);  // Load Options Menu
    }

    public void Instructions()
    {
        AudioManager.instance.PlayButtonClick();
        SceneManager.LoadSceneAsync(1);  // Load Instructions Screen
    }

    public void Achievements()
    {
        AudioManager.instance.PlayButtonClick();
        SceneManager.LoadSceneAsync(3);  // Load Achievements Screen
    }

    public void Home()
    {
        AudioManager.instance.PlayButtonClick();
        SceneManager.LoadSceneAsync(0);  // Load Main Menu
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
        SceneManager.LoadSceneAsync(5);  // Load Keyboard Settings
    }
}