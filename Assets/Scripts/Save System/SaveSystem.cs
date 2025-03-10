using System;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
}

public class SaveSystem : MonoBehaviour
{
    private static string savePath => Application.persistentDataPath + "/savegame.json";
    private static string loadFlagPath => Application.persistentDataPath + "/loadflag.txt";  

    public static void SaveGame(Vector3 playerPosition)
    {
        SaveData data = new SaveData
        {
            playerPosition = playerPosition
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        // Indicate a saved game exists
        File.WriteAllText(loadFlagPath, "true");

        Debug.Log("Game Saved: " + savePath);
    }

    public static Vector3? LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file found.");
            return null;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log("Game Loaded!");
        return data.playerPosition;
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
