using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Helper.Waiter;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Vector3Int currentPlayerChunkPosition;
    private Vector3Int currentChunkCenter = Vector3Int.zero;

    [SerializeField] NavMeshSurface navMeshSurface;
    public DayNightCycleManager dayNightCycleManager;
    public World world;

    public float detectionTime = 1;
    public CinemachineCamera camera_VM;
    public CinemachineBrain cameraBrain;
    public GameObject player { get; private set; }

    [Header("Enemy")]
    public Enemy enemyPrefab;
    public int enemyCount = 10;
    public Vector2 enemySpawnRangeX = new Vector2(-100, 100);
    public Vector2 enemySpawnRangeY = new Vector2(-100, 100);
    List<Enemy> enemies = new List<Enemy>();


    private void Start()
    {
        // Ensure time scale is set to 1 at start
        Time.timeScale = 1;

        if (SaveSystem.ShouldLoadGame())
        {
            if (world == null)
            {
                world = FindObjectOfType<World>();
                if (world == null)
                {
                    Debug.LogError("World component not found in scene.");
                    return;
                }
            }
            else
            {
                // Show loading screen before starting a new game
                if (LoadingScreen.Instance != null)
                    LoadingScreen.Instance.ShowLoadingScreen();

                StartNewGame();
            }
            // Initialize worldData if needed
            if (world.worldData == null)
            {
                world.worldData = new WorldData
                {
                    chunkHeight = world.chunkHeight,
                    chunkSize = world.chunkSize,
                    chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
                    chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
                };
            }

            // Load the world first
            Vector3? savedPosition = null;
            int worldSeed; // Declare variable to store worldSeed
            if (SaveSystem.LoadGame(world, out savedPosition, out worldSeed)) // Pass worldSeed
            {
                world.worldSeed = worldSeed; // Ensure the loaded world uses the correct seed

                // Wait for world to finish generating before spawning player
                world.OnWorldCreated.AddListener(() => {
                    // Then spawn the player at the saved position
                    player = GameObject.FindWithTag("Player");
                    if (player == null)
                    {
                        player = Instantiate(playerPrefab, savedPosition.Value, Quaternion.identity);
                        player.tag = "Player";
                    }
                    else
                    {
                        player.transform.position = savedPosition.Value;
                    }

                    Debug.Log("Loaded saved position at: " + savedPosition.Value);

                    // Setup player and camera references
                    SetupPlayerReferences();

                    // Rebuild the NavMesh for AI navigation
                    if (navMeshSurface != null)
                    {
                        navMeshSurface.BuildNavMesh();
                    }

                    // Start checking map position
                    StartCheckingTheMap();
                });
            }
            else
            {
                Debug.LogError("Failed to load game. Starting new game instead.");
                StartNewGame();
            }

        }
        else
        {
            StartNewGame();
        }
    }
    private void SetupPlayerReferences()
    {
        if (player != null)
        {
            // Connect camera to player
            if (camera_VM != null)
            {
                camera_VM.Follow = player.transform.GetChild(0);
            }

            if (cameraBrain != null)
            {
                cameraBrain.enabled = true;
            }

            // Enable player controls if they have a controller component
            var playerController = player.GetComponent<PlayerMovement>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            // Make sure loading screen is hidden when player is ready
            if (LoadingScreen.Instance != null)
                LoadingScreen.Instance.HideLoadingScreen();
        }
    }

    private void OnDestroy()
    {
        dayNightCycleManager.OnLightStateChange.RemoveAllListeners();
    }

    public void StartNewGame()
    {
        SaveSystem.DeleteSave();
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (player != null) return;

        // Check if a player already exists in the scene
        player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            // If no player exists, spawn a new one
            Vector3Int raycastStartposition = new Vector3Int(world.chunkSize / 2, 100, world.chunkSize / 2);
            if (Physics.Raycast(raycastStartposition, Vector3.down, out RaycastHit hit, 120))
            {
                player = Instantiate(playerPrefab, hit.point + Vector3Int.up, Quaternion.identity);
                player.tag = "Player";  // Make sure your player has the "Player" tag

                SetupPlayerReferences();

                StartCheckingTheMap();

                // Build the navmesh for AI navigation
                if (navMeshSurface != null)
                {
                    navMeshSurface.BuildNavMesh();
                }

                // Spawn enemies after a delay
                Waiter.Wait(5f, () =>
                {
                    if (dayNightCycleManager.activateLights)
                    {
                        SpawnEnemies();
                    }
                    else
                    {
                        dayNightCycleManager.OnLightStateChange.RemoveAllListeners();
                        dayNightCycleManager.OnLightStateChange.AddListener((isLightOn) =>
                        {
                            if (isLightOn)
                            {
                                SpawnEnemies();
                            }
                        });
                    }
                });
            }
        }
        else
        {
            SetupPlayerReferences();
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(enemySpawnRangeX.x, enemySpawnRangeY.y), 100, Random.Range(enemySpawnRangeY.x, enemySpawnRangeY.y));

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 120))
            {
                spawnPosition = hit.point + (Vector3.up * 2);
            }

            Enemy enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.Init(player.transform);
            enemies.Add(enemy);
        }
    }

    public void StartCheckingTheMap()
    {
        SetCurrentChunkCoordinates();
        StopAllCoroutines();
        StartCoroutine(CheckIfShouldLoadNextPosition());
    }

    IEnumerator CheckIfShouldLoadNextPosition()
    {
        yield return new WaitForSeconds(detectionTime);
        if (Mathf.Abs(currentChunkCenter.x - player.transform.position.x) > world.chunkSize ||
            Mathf.Abs(currentChunkCenter.z - player.transform.position.z) > world.chunkSize ||
            Mathf.Abs(currentPlayerChunkPosition.y - player.transform.position.y) > world.chunkHeight)
        {
            world.LoadAdditionalChunksRequest(player);
        }
        else
        {
            StartCoroutine(CheckIfShouldLoadNextPosition());
        }
    }

    private void SetCurrentChunkCoordinates()
    {
        currentPlayerChunkPosition = WorldDataHelper.ChunkPositionFromBlockCoords(world, Vector3Int.RoundToInt(player.transform.position));
        currentChunkCenter.x = currentPlayerChunkPosition.x + world.chunkSize / 2;
        currentChunkCenter.z = currentPlayerChunkPosition.z + world.chunkSize / 2;
    }
}