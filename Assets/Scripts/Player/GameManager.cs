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
        if (SaveSystem.ShouldLoadGame())
        {
            if (world == null || world.worldData.chunkDataDictionary == null)
            {
                Debug.LogError("World is not initialized. Make sure the World component is properly set up.");
                return;
            }


            if (SaveSystem.LoadGame(world, out Vector3? savedPosition))
            {
                SpawnPlayer();

                if (savedPosition.HasValue && player != null)
                {
                    player.transform.position = savedPosition.Value;
                    Debug.Log("Loaded saved position and world data.");
                }
                else
                {
                    Debug.Log("No saved position found. Starting new game.");
                }
            }
        }
        else
        {
            StartNewGame();
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

                camera_VM.Follow = player.transform.GetChild(0);
                cameraBrain.enabled = true;
                StartCheckingTheMap();

                // Build the navmesh for AI navigation
                navMeshSurface.BuildNavMesh();

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
