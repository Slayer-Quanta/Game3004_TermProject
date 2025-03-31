using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public ChunkEntry[] worldChunksArray; // Changed to array for serialization
    public int worldSeed; // Added to store the world seed
}

[System.Serializable]
public class ChunkEntry // Helper class for serialization
{
    public int posX;
    public int posY;
    public int posZ;
    public ChunkSaveData chunkData;
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
        List<ChunkEntry> chunkEntries = new List<ChunkEntry>();
        foreach (var chunk in world.worldData.chunkDataDictionary)
        {
            ChunkSaveData chunkSaveData = new ChunkSaveData
            {
                blocks = chunk.Value.blocks
            };

            chunkEntries.Add(new ChunkEntry
            {
                posX = chunk.Key.x,
                posY = chunk.Key.y,
                posZ = chunk.Key.z,
                chunkData = chunkSaveData
            });
        }

        SaveData data = new SaveData
        {
            playerPosition = playerPosition,
            worldChunksArray = chunkEntries.ToArray(),
            worldSeed = world.worldSeed // Save the current world seed
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        File.WriteAllText(loadFlagPath, "true");

        Debug.Log($"Game Saved: {savePath} with seed: {world.worldSeed}");
    }

    public static bool LoadGame(World world, out Vector3? playerPosition, out int worldSeed)
    {
        playerPosition = null;
        worldSeed = 0;

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

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            playerPosition = data.playerPosition;
            worldSeed = data.worldSeed; // Load the saved world seed

            // Clear existing world data
            world.worldData.chunkDataDictionary.Clear();
            world.worldData.chunkDictionary.Clear();

            // Convert array back to dictionary
            foreach (var entry in data.worldChunksArray)
            {
                Vector3Int chunkPos = new Vector3Int(entry.posX, entry.posY, entry.posZ);

                // Create a new chunk and restore its block data
                ChunkData newChunk = new ChunkData(world.chunkSize, world.chunkHeight, world, chunkPos);
                newChunk.blocks = entry.chunkData.blocks;

                world.worldData.chunkDataDictionary.Add(chunkPos, newChunk);
            }

            // Regenerate the world around player position
            Vector3Int playerChunkPos = Vector3Int.FloorToInt(data.playerPosition);
            world.RegenerateWorldFromSaveData(playerChunkPos);

            Debug.Log($"Game Loaded Successfully with seed: {worldSeed}!");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading game: " + e.Message);
            return false;
        }
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