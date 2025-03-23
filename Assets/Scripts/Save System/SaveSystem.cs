using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public Dictionary<Vector3Int, ChunkSaveData> worldChunks;
}

[System.Serializable]
public class ChunkSaveData
{
    public BlockType[] blocks; 
}

public class SaveSystem : MonoBehaviour
{
    private static string savePath => Application.persistentDataPath + "/savegame.json";
    private static string loadFlagPath => Application.persistentDataPath + "/loadflag.txt";

    public static void SaveGame(Vector3 playerPosition, World world)
    {
        SaveData data = new SaveData
        {
            playerPosition = playerPosition,
            worldChunks = new Dictionary<Vector3Int, ChunkSaveData>()
        };

        foreach (var chunk in world.worldData.chunkDataDictionary)
        {
            ChunkSaveData chunkSaveData = new ChunkSaveData
            {
                blocks = chunk.Value.blocks 
            };
            data.worldChunks.Add(chunk.Key, chunkSaveData);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        File.WriteAllText(loadFlagPath, "true");

        Debug.Log("Game Saved: " + savePath);
    }

    public static bool LoadGame(World world, out Vector3? playerPosition)
    {
        playerPosition = null;

        if (!File.Exists(savePath))
        {
            Debug.Log("No save file found.");
            return false;
        }

        if (world == null || world.worldData.chunkDataDictionary == null)
        {
            Debug.LogError("World or WorldData is not initialized.");
            return false;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        playerPosition = data.playerPosition;

        // Clear existing world data
        world.worldData.chunkDataDictionary.Clear();

        foreach (var chunkEntry in data.worldChunks)
        {
            Vector3Int chunkPos = chunkEntry.Key;
            ChunkSaveData savedChunk = chunkEntry.Value;

            // Create a new chunk and restore its block data
            ChunkData newChunk = new ChunkData(world.chunkSize, world.chunkHeight, world, chunkPos);
            newChunk.blocks = savedChunk.blocks;

            world.worldData.chunkDataDictionary.Add(chunkPos, newChunk);
        }

        Debug.Log("Game Loaded!");
        return true;
    }


    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save Deleted.");
        }

        if (File.Exists(loadFlagPath))
        {
            File.Delete(loadFlagPath);
            Debug.Log("Load Flag Deleted.");
        }
    }

    public static bool ShouldLoadGame()
    {
        return File.Exists(loadFlagPath);
    }
}
