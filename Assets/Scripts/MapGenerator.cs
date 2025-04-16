using System.Collections;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private LoadingScreen loadingManager;
    [SerializeField] private int totalSteps = 10; // Define how many steps your generation process has

    public void GenerateMap()
    {
        // Show loading screen before starting generation
        loadingManager.ShowLoadingScreen();

        // Start map generation as coroutine
        StartCoroutine(GenerateMapAsync());
    }

    private IEnumerator GenerateMapAsync()
    {
        // Track generation progress
        float progress = 0f;

        // Map generation steps
        for (int i = 0; i < totalSteps; i++)
        {
            // Perform a chunk of generation work
            PerformGenerationStep(i);

            // Update progress
            progress = (float)(i + 1) / totalSteps;
            loadingManager.UpdateProgress(progress);

            // Yield to prevent freezing
            yield return null;
        }

        // Hide loading screen when finished
        loadingManager.HideLoadingScreen();
    }

    private void PerformGenerationStep(int stepIndex)
    {
        
        switch (stepIndex)
        {
            case 0:
                // Generate terrain heightmap
                Debug.Log("Generating terrain heightmap");
                break;
            case 1:
                // Place trees
                Debug.Log("Placing trees");
                break;
            case 2:
                // Generate buildings
                Debug.Log("Generating buildings");
                break;
                // Add more steps as needed
        }
    }
}