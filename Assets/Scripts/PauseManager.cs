//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class PauseManager : MonoBehaviour
//{
//    [SerializeField] private GameObject pauseMenu;
//    [SerializeField] private GameObject pauseButton;


//    private bool isPaused = false;

//    public void PauseGame()
//    {
//        if (isPaused) return;

//        isPaused = true;
//        pauseMenu.SetActive(true);
//        pauseButton.SetActive(false); // Hide Pause Button
//        Time.timeScale = 0f;
//    }

//    public void ResumeGame()
//    {
//        if (!isPaused) return;

//        isPaused = false;
//        pauseMenu.SetActive(false);
//        pauseButton.SetActive(true); // Show Pause Button
//        Time.timeScale = 1f;
//    }

//    public void SaveGame()
//    {
       
//            Debug.Log("Game Saved!");
   
//    }

//    public void LoadGame()
//    {
     
//            Debug.Log("Game Loaded!");
      
//    }

//    public void QuitToMainMenu()
//    {
//        Time.timeScale = 1f;
//        SceneManager.LoadSceneAsync(0);
//    }
//}
