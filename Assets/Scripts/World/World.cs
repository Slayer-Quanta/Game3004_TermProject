using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;

public class World : MonoBehaviour
{
    public static World self;

    public int mapSizeInChunks = 6;
    public int chunkSize = 16, chunkHeight = 100;
    public int chunkDrawingRange = 8;

    public GameObject chunkPrefab;
    public WorldRenderer worldRenderer;

    public TerrainGenerator terrainGenerator;
    public Vector2Int mapSeedOffset;
    public GameObject Menu;
    public GameObject minimapborder;

    [HideInInspector]
    public int worldSeed;
    //// Seed related settings
    //[SerializeField]
    //private bool useRandomSeed = true;
    //[SerializeField]
    //private int customSeed = 0;

    CancellationTokenSource taskTokenSource = new CancellationTokenSource();

    //public Dictionary<Vector3Int, ChunkData> chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>();
    //public Dictionary<Vector3Int, ChunkRenderer> chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>();

    public UnityEvent OnWorldCreated, OnNewChunksGenerated;

    public WorldData worldData { get; set; } 

    public bool IsWorldCreated { get; private set; }

    private void Awake()
    {
        self = this;

        // Initialize worldData properly
        if (worldData == null)
        {
            worldData = new WorldData
            {
                chunkHeight = this.chunkHeight,
                chunkSize = this.chunkSize,
                chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
                chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
            };
        }
    }


    void Start()
    {
        GenerateWorld();
        if (Menu != null) Menu.SetActive(false);
        if (minimapborder != null) minimapborder.SetActive(true);
    }
    private void InitializeSeed()
    {
        // Always generate a new random seed
        worldSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        // Use the seed to initialize the random number generator
        UnityEngine.Random.InitState(worldSeed);

        // Set the mapSeedOffset based on the seed
        mapSeedOffset = new Vector2Int(
            worldSeed % 10000,
            (worldSeed / 10000) % 10000
        );

        Debug.Log($"World initialized with seed: {worldSeed}");
    }
    public async void GenerateWorld()
    {
        // Show loading screen
        if (LoadingScreen.Instance != null)
            LoadingScreen.Instance.ShowLoadingScreen();

        if (SaveSystem.ShouldLoadGame())
        {
            Vector3? loadedPosition = null;
            int loadedSeed = 0;
            if (SaveSystem.LoadGame(this, out loadedPosition, out loadedSeed) && loadedPosition.HasValue)
            {
                worldSeed = loadedSeed;

                mapSeedOffset = new Vector2Int(
                    worldSeed % 10000,
                    (worldSeed / 10000) % 10000
                );

                Debug.Log($"Loaded existing world with seed: {worldSeed}");

                if (LoadingScreen.Instance != null)
                    LoadingScreen.Instance.HideLoadingScreen();

                return;
            }
        }

        InitializeSeed();
        await GenerateWorld(Vector3Int.zero);
    }

    private async Task GenerateWorld(Vector3Int position)
    {
        // Update loading progress - Terrain generation (25%)
        if (LoadingScreen.Instance != null)
            LoadingScreen.Instance.UpdateProgress(0.25f);

        // The rest of your GenerateWorld method
        terrainGenerator.GenerateBiomePoints(position, chunkDrawingRange, chunkSize, mapSeedOffset);

        WorldGenerationData worldGenerationData = await Task.Run(() => GetPositionsThatPlayerSees(position), taskTokenSource.Token);

        // Update loading progress - Chunk data (50%)
        if (LoadingScreen.Instance != null)
            LoadingScreen.Instance.UpdateProgress(0.5f);

        foreach (Vector3Int pos in worldGenerationData.chunkPositionsToRemove)
        {
            WorldDataHelper.RemoveChunk(this, pos);
        }

        foreach (Vector3Int pos in worldGenerationData.chunkDataToRemove)
        {
            WorldDataHelper.RemoveChunkData(this, pos);
        }

        ConcurrentDictionary<Vector3Int, ChunkData> dataDictionary = null;

        try
        {
            dataDictionary = await CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate);
        }
        catch (Exception)
        {
            Debug.Log("Task canceled");

            // Hide loading screen if task is canceled
            if (LoadingScreen.Instance != null)
                LoadingScreen.Instance.HideLoadingScreen();

            return;
        }

        // Update loading progress - Processing chunks (75%)
        if (LoadingScreen.Instance != null)
            LoadingScreen.Instance.UpdateProgress(0.75f);

        foreach (var calculatedData in dataDictionary)
        {
            worldData.chunkDataDictionary.Add(calculatedData.Key, calculatedData.Value);
        }
        foreach (var chunkData in worldData.chunkDataDictionary.Values)
        {
            AddTreeLeafs(chunkData);
        }

        ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

        List<ChunkData> dataToRender = worldData.chunkDataDictionary
            .Where(keyvaluepair => worldGenerationData.chunkPositionsToCreate.Contains(keyvaluepair.Key))
            .Select(keyvalpair => keyvalpair.Value)
            .ToList();

        try
        {
            meshDataDictionary = await CreateMeshDataAsync(dataToRender);
        }
        catch (Exception)
        {
            Debug.Log("Task canceled");

            // Hide loading screen if task is canceled
            if (LoadingScreen.Instance != null)
                LoadingScreen.Instance.HideLoadingScreen();

            return;
        }

        // Update loading progress - Final stages (90%)
        if (LoadingScreen.Instance != null)
            LoadingScreen.Instance.UpdateProgress(0.9f);

        StartCoroutine(ChunkCreationCoroutine(meshDataDictionary));
    }

    private void AddTreeLeafs(ChunkData chunkData)
	{
		foreach (var treeLeafes in chunkData.treeData.treeLeafesSolid)
		{
			Chunk.SetBlock(chunkData, treeLeafes, BlockType.TreeLeafsSolid);
		}
	}

	private Task<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
	{
		ConcurrentDictionary<Vector3Int, MeshData> dictionary = new ConcurrentDictionary<Vector3Int, MeshData>();
		return Task.Run(() =>
		{

			foreach (ChunkData data in dataToRender)
			{
				if (taskTokenSource.Token.IsCancellationRequested)
				{
					taskTokenSource.Token.ThrowIfCancellationRequested();
				}
				MeshData meshData = Chunk.GetChunkMeshData(data);
				dictionary.TryAdd(data.worldPosition, meshData);
			}

			return dictionary;
		}, taskTokenSource.Token
		);
	}

	private Task<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(List<Vector3Int> chunkDataPositionsToCreate)
	{
		ConcurrentDictionary<Vector3Int, ChunkData> dictionary = new ConcurrentDictionary<Vector3Int, ChunkData>();

		return Task.Run(() =>
		{
			foreach (Vector3Int pos in chunkDataPositionsToCreate)
			{
				if (taskTokenSource.Token.IsCancellationRequested)
				{
					taskTokenSource.Token.ThrowIfCancellationRequested();
				}
				ChunkData data = new ChunkData(chunkSize, chunkHeight, this, pos);
				ChunkData newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
				dictionary.TryAdd(pos, newData);
			}
			return dictionary;
		},
		taskTokenSource.Token
		);


	}

    IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary)
    {
        foreach (var item in meshDataDictionary)
        {
            CreateChunk(worldData, item.Key, item.Value);
            yield return new WaitForEndOfFrame();
        }

        // Update loading progress - Complete (100%)
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.UpdateProgress(1.0f);
            // Wait a brief moment at 100% before hiding
            yield return new WaitForSeconds(0.5f);
            LoadingScreen.Instance.HideLoadingScreen();
        }

        if (IsWorldCreated == false)
        {
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }
    }
    private void CreateChunk(WorldData worldData, Vector3Int position, MeshData meshData)
    {
        ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(worldData, position, meshData);
        worldData.chunkDictionary.Add(position, chunkRenderer);

        // After a chunk is created or modified, update its mesh
        chunkRenderer.UpdateChunk();
        chunkRenderer.BuildNavMeshForChunk();
    }
    public void BuildGlobalNavMesh()
    {
        NavMeshSurface navMeshSurface = FindObjectOfType<NavMeshSurface>();
        if (navMeshSurface != null)
        {
            try
            {
                navMeshSurface.BuildNavMesh();
                Debug.Log("Built global NavMesh");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error building NavMesh: {e.Message}");
            }
        }
    }
    internal bool SetBlock(RaycastHit hit, BlockType blockType)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
            return false;

        Vector3Int pos = GetBlockPos(hit);

        WorldDataHelper.SetBlock(chunk.ChunkData.worldReference, pos, blockType);
        chunk.ModifiedByThePlayer = true;

        if (Chunk.IsOnEdge(chunk.ChunkData, pos))
        {
            List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
            foreach (ChunkData neighbourData in neighbourDataList)
            {
                //neighbourData.modifiedByThePlayer = true;
                ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);
                if (chunkToUpdate != null)
                {
                    chunkToUpdate.UpdateChunk();
                    chunkToUpdate.BuildNavMeshForChunk(); // Add here for neighbor chunks
                }
            }
        }

        chunk.UpdateChunk();
        chunk.BuildNavMeshForChunk(); // Add here for the modified chunk
        return true;
    }

    private Vector3Int GetBlockPos(RaycastHit hit)
	{
		Vector3 pos = new Vector3(
			 GetBlockPositionIn(hit.point.x, hit.normal.x),
			 GetBlockPositionIn(hit.point.y, hit.normal.y),
			 GetBlockPositionIn(hit.point.z, hit.normal.z)
			 );

		return Vector3Int.RoundToInt(pos);
	}

	private float GetBlockPositionIn(float pos, float normal)
	{
		if (Mathf.Abs(pos % 1) == 0.5f)
		{
			pos -= (normal / 2);
		}


		return (float)pos;
	}
    internal bool SetBlock(Vector3Int position, BlockType blockType)
    {
        // Get the chunk at the given position
        Vector3Int chunkCoord = Chunk.ChunkPositionFromBlockCoords(this, position.x, position.y, position.z);

        // Check if the chunk exists
        if (worldData.chunkDataDictionary.TryGetValue(chunkCoord, out ChunkData chunkData))
        {
            // Set the block at the calculated position
            WorldDataHelper.SetBlock(chunkData.worldReference, position, blockType);

            // Mark chunk as modified and update it
            ChunkRenderer chunkRenderer = WorldDataHelper.GetChunk(chunkData.worldReference, chunkData.worldPosition);
            if (chunkRenderer != null)
            {
                chunkRenderer.ModifiedByThePlayer = true;
                chunkRenderer.UpdateChunk();
                chunkRenderer.BuildNavMeshForChunk();
            }

            return true;
        }

        return false;
    }

    private WorldGenerationData GetPositionsThatPlayerSees(Vector3Int playerPosition)
	{
		List<Vector3Int> allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsAroundPlayer(this, playerPosition);

		List<Vector3Int> allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsAroundPlayer(this, playerPosition);

		List<Vector3Int> chunkPositionsToCreate = WorldDataHelper.SelectPositonsToCreate(worldData, allChunkPositionsNeeded, playerPosition);
		List<Vector3Int> chunkDataPositionsToCreate = WorldDataHelper.SelectDataPositonsToCreate(worldData, allChunkDataPositionsNeeded, playerPosition);

		List<Vector3Int> chunkPositionsToRemove = WorldDataHelper.GetUnnededChunks(worldData, allChunkPositionsNeeded);
		List<Vector3Int> chunkDataToRemove = WorldDataHelper.GetUnnededData(worldData, allChunkDataPositionsNeeded);

		WorldGenerationData data = new WorldGenerationData
		{
			chunkPositionsToCreate = chunkPositionsToCreate,
			chunkDataPositionsToCreate = chunkDataPositionsToCreate,
			chunkPositionsToRemove = chunkPositionsToRemove,
			chunkDataToRemove = chunkDataToRemove,
			chunkPositionsToUpdate = new List<Vector3Int>()
		};
		return data;

	}

	internal async void LoadAdditionalChunksRequest(GameObject player)
	{
		Debug.Log("Load more chunks");
		await GenerateWorld(Vector3Int.RoundToInt(player.transform.position));
		OnNewChunksGenerated?.Invoke();
	}

	internal BlockType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
	{
		Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, x, y, z);
		ChunkData containerChunk = null;

		worldData.chunkDataDictionary.TryGetValue(pos, out containerChunk);

		if (containerChunk == null)
			return BlockType.Nothing;
		Vector3Int blockInCHunkCoordinates = Chunk.GetBlockInChunkCoordinates(containerChunk, new Vector3Int(x, y, z));
		return Chunk.GetBlockFromChunkCoordinates(containerChunk, blockInCHunkCoordinates);
	}

    private bool FindExistingPlayer()
    {
        GameObject existingPlayer = GameObject.FindWithTag("Player");

        if (existingPlayer != null)
        {
            return true; 
        }

        return false; 
    }
    public async void RegenerateWorldFromSaveData(Vector3Int playerPosition)
    {
        // Calculate which chunks need to be created based on loaded data
        WorldGenerationData worldGenerationData = await Task.Run(() => GetPositionsThatPlayerSees(playerPosition), taskTokenSource.Token);

        // Create mesh data for the loaded chunks
        ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

        List<ChunkData> dataToRender = new List<ChunkData>();
        foreach (var chunkPos in worldGenerationData.chunkPositionsToCreate)
        {
            if (worldData.chunkDataDictionary.TryGetValue(chunkPos, out ChunkData chunkData))
            {
                dataToRender.Add(chunkData);
            }
        }

        try
        {
            meshDataDictionary = await CreateMeshDataAsync(dataToRender);
        }
        catch (Exception)
        {
            Debug.Log("Task canceled");
            return;
        }

        StartCoroutine(ChunkCreationCoroutine(meshDataDictionary));

        // Set world created flag if needed
        if (IsWorldCreated == false)
        {
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }
    }

    public void OnDisable()
	{
		taskTokenSource.Cancel();
	}

	public struct WorldGenerationData
	{
		public List<Vector3Int> chunkPositionsToCreate;
		public List<Vector3Int> chunkDataPositionsToCreate;
		public List<Vector3Int> chunkPositionsToRemove;
		public List<Vector3Int> chunkDataToRemove;
		public List<Vector3Int> chunkPositionsToUpdate;
	}


}
public class WorldData
{
	public Dictionary<Vector3Int, ChunkData> chunkDataDictionary;
	public Dictionary<Vector3Int, ChunkRenderer> chunkDictionary;
	public int chunkSize;
	public int chunkHeight;
}
