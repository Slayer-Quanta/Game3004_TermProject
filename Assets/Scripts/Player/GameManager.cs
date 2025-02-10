using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public GameObject playerPrefab;
    public Vector3Int currentPlayerChunkPosition;
	private Vector3Int currentChunkCenter = Vector3Int.zero;

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

            SpawnEnemies();
        }
	}

    void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(enemySpawnRangeX.x, enemySpawnRangeX.y), 100, Random.Range(enemySpawnRangeY.x, enemySpawnRangeY.y));
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