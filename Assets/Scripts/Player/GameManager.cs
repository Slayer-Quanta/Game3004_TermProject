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

    private void OnDestroy()
    {
        dayNightCycleManager.OnLightStateChange.RemoveAllListeners();
    }

    public void SpawnPlayer()
	{
		if (player != null)
			return;
		Vector3Int raycastStartposition = new Vector3Int(world.chunkSize / 2, 100, world.chunkSize / 2);
		RaycastHit hit;
		if (Physics.Raycast(raycastStartposition, Vector3.down, out hit, 120))
		{

			player = Instantiate(playerPrefab, hit.point + Vector3Int.up, Quaternion.identity);
			camera_VM.Follow = player.transform.GetChild(0);
			cameraBrain.enabled = true;
			StartCheckingTheMap();

            // Build the navmesh
            navMeshSurface.BuildNavMesh();

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



    void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(enemySpawnRangeX.x, enemySpawnRangeX.y), 100, Random.Range(enemySpawnRangeY.x, enemySpawnRangeY.y));

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
		if (
			Mathf.Abs(currentChunkCenter.x - player.transform.position.x) > world.chunkSize ||
			Mathf.Abs(currentChunkCenter.z - player.transform.position.z) > world.chunkSize ||
			(Mathf.Abs(currentPlayerChunkPosition.y - player.transform.position.y) > world.chunkHeight)
			)
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