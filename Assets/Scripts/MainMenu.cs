using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private GameObject optionsPanel;
	[SerializeField] private GameObject settingsPanel;
	public void PlayGame()
     {
        SceneManager.LoadSceneAsync(4);
     }
	public void Options()
	{
		SceneManager.LoadSceneAsync(2);
	}
	public void Instructions()
	{
		SceneManager.LoadSceneAsync(1);
	}
	public void Achievements()
	{
		SceneManager.LoadSceneAsync(3);
	}
	public void Home()
	{
		SceneManager.LoadSceneAsync(0);
	}
	public void Exit()
	{
		Application.Quit();
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			QuitGame();
		}
	}

	void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.ExitPlaymode(); // Stops play mode in the Unity Editor
#else
        Application.Quit(); // Closes the game in a built application
#endif

		Debug.Log("Game is exiting...");
	}
	public void ShowSettingsPanel()
	{
		optionsPanel.SetActive(false); 
		settingsPanel.SetActive(true); 
	}

	public void ShowOptionsPanel()
	{
		settingsPanel.SetActive(false); 
		optionsPanel.SetActive(true); 
	}

	
}
