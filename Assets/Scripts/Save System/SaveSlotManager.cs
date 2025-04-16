//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;
//using System.Collections.Generic;

//public class SaveSlotManager : MonoBehaviour
//{
//    // Singleton instance
//    public static SaveSlotManager Instance { get; private set; }

//    [SerializeField] private Transform slotContainer;           // Parent UI container for slots
//    [SerializeField] private GameObject saveSlotPrefab;         // The save slot UI prefab
//    [SerializeField] private GameObject worldEntryPrefab;       // World entry UI prefab
//    [SerializeField] private Transform worldListContainer;      // Container for world entries
//    [SerializeField] private GameObject worldCreationPanel;     // Panel for creating new worlds
//    [SerializeField] private TMP_InputField worldNameInput;     // Input for new world name
//    [SerializeField] private TMP_InputField seedInput;          // Input for world seed (optional)
//    [SerializeField] private GameObject noWorldsMessage;        // Message shown when no worlds exist
//    [SerializeField] private GameObject mainMenuPanel;          // Main menu panel reference

//    private List<SaveSlotUI> saveSlots = new List<SaveSlotUI>();
//    private List<WorldEntryUI> worldEntries = new List<WorldEntryUI>();

//    private int selectedSlot = 0;
//    public string selectedWorldId = "";
//    private float currentPlayTime = 0f;
//    private bool isNewGame = false;

//    // Remove serialized reference to GameManager
//    private GameManager gameManager;

//    private void Awake()
//    {
//        // Singleton pattern implementation
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    private void Start()
//    {
//        LoadSaveSlots();

//        // Default hide world creation panel
//        if (worldCreationPanel != null)
//            worldCreationPanel.SetActive(false);
//    }

//    private void Update()
//    {
//        if (Time.timeScale > 0) // Only count time when game is not paused
//            currentPlayTime += Time.deltaTime;
//    }

//    public void SaveCurrentGame()
//    {
//        // Find GameManager at runtime if needed
//        if (gameManager == null)
//            gameManager = FindObjectOfType<GameManager>();

//        if (gameManager != null && gameManager.player != null)
//        {
//            SaveSystem.SaveGame(
//                gameManager.player.transform.position,
//                gameManager.world,
//                currentPlayTime
//            );

//            // Refresh UI if visible
//            if (gameObject.activeInHierarchy)
//                LoadWorldsForSlot(selectedSlot);
//        }
//    }

//    public void LoadSaveSlots()
//    {
//        // Clear existing slots
//        foreach (Transform child in slotContainer)
//        {
//            Destroy(child.gameObject);
//        }
//        saveSlots.Clear();

//        // Get all available slots
//        List<SaveSlot> availableSlots = SaveSystem.GetAllSaveSlots();

//        // Create UI for each slot
//        for (int i = 0; i < availableSlots.Count; i++)
//        {
//            GameObject newSlotObj = Instantiate(saveSlotPrefab, slotContainer);
//            SaveSlotUI slotUI = newSlotObj.GetComponent<SaveSlotUI>();

//            if (slotUI != null)
//            {
//                slotUI.Setup(i, this, availableSlots[i].worlds.Count);
//                saveSlots.Add(slotUI);
//            }
//        }

//        // Select first slot by default
//        SelectSlot(0);
//    }

//    public void SelectSlot(int slotIndex)
//    {
//        selectedSlot = slotIndex;

//        // Update UI selection state
//        foreach (var slot in saveSlots)
//        {
//            slot.SetSelected(slot.SlotId == slotIndex);
//        }

//        // Load worlds for this slot
//        LoadWorldsForSlot(slotIndex);
//    }

//    public void RefreshSaveSlots()
//    {
//        // Clear caches
//        selectedWorldId = "";

//        LoadSaveSlots();
//    }

//    private void OnEnable()
//    {
//        // No need to find SaveSlotManager anymore since we have the singleton
//        RefreshSaveSlots();
//    }

//    public void LoadWorldsForSlot(int slotIndex)
//    {
//        // Clear existing world entries
//        foreach (Transform child in worldListContainer)
//        {
//            Destroy(child.gameObject);
//        }
//        worldEntries.Clear();

//        // Get worlds for this slot
//        List<WorldSave> worlds = SaveSystem.GetWorldsInSlot(slotIndex);

//        if (worlds.Count == 0)
//        {
//            // Show no worlds message
//            if (noWorldsMessage != null)
//                noWorldsMessage.SetActive(true);
//        }
//        else
//        {
//            // Hide no worlds message
//            if (noWorldsMessage != null)
//                noWorldsMessage.SetActive(false);

//            // Create UI for each world
//            foreach (var world in worlds)
//            {
//                GameObject worldEntryObj = Instantiate(worldEntryPrefab, worldListContainer);
//                WorldEntryUI worldUI = worldEntryObj.GetComponent<WorldEntryUI>();

//                if (worldUI != null)
//                {
//                    worldUI.Setup(world, this);
//                    worldEntries.Add(worldUI);
//                }
//            }
//        }
//    }

//    public void SelectWorld(string worldId)
//    {
//        selectedWorldId = worldId;

//        // Update UI selection state
//        foreach (var entry in worldEntries)
//        {
//            entry.SetSelected(entry.WorldId == worldId);
//        }
//    }

//    public void ShowWorldCreationPanel()
//    {
//        if (worldCreationPanel != null)
//        {
//            worldCreationPanel.SetActive(true);

//            // Reset inputs
//            if (worldNameInput != null)
//                worldNameInput.text = $"World {System.DateTime.Now.ToString("yyyyMMdd")}";

//            if (seedInput != null)
//                seedInput.text = Random.Range(1, 999999).ToString();
//        }
//    }

//    public void HideWorldCreationPanel()
//    {
//        if (worldCreationPanel != null)
//            worldCreationPanel.SetActive(false);
//    }

//    public void CreateNewWorld()
//    {
//        string worldName = worldNameInput != null ? worldNameInput.text : "New World";

//        if (string.IsNullOrEmpty(worldName))
//            worldName = $"World {System.DateTime.Now.ToString("yyyyMMdd")}";

//        // Parse seed
//        int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
//        if (seedInput != null && !string.IsNullOrEmpty(seedInput.text))
//        {
//            if (!int.TryParse(seedInput.text, out seed))
//            {
//                // Use string hash as seed if not a valid number
//                seed = worldName.GetHashCode();
//            }
//        }

//        // Create world settings with default values
//        WorldSettings settings = new WorldSettings
//        {
//            chunkSize = 16,
//            chunkHeight = 100,
//            difficulty = 1,
//            enableMobs = true,
//            enableWeather = true
//        };

//        // Create the world
//        string worldId = SaveSystem.CreateNewWorld(worldName, seed, settings);

//        // Flag as new game
//        isNewGame = true;

//        // IMPORTANT: Update the selectedWorldId with the new worldId
//        selectedWorldId = worldId;

//        // Set as current world and start game
//        SaveSystem.SetActiveSlot(selectedSlot, worldId);

//        HideWorldCreationPanel();
//        StartSelectedWorld();
//    }

//    public void StartSelectedWorld()
//    {
//        if (string.IsNullOrEmpty(selectedWorldId))
//            return;

//        // Set the active world and slot
//        SaveSystem.SetActiveSlot(selectedSlot, selectedWorldId);

//        // Hide UI
//        gameObject.SetActive(false);
//        if (mainMenuPanel != null)
//            mainMenuPanel.SetActive(false);

//        UnityEngine.SceneManagement.SceneManager.LoadScene(4);
//    }

//    public void DeleteSelectedWorld()
//    {
//        if (string.IsNullOrEmpty(selectedWorldId))
//            return;

//        SaveSystem.DeleteWorld(selectedSlot, selectedWorldId);
//        selectedWorldId = "";

//        // Refresh worlds list
//        LoadWorldsForSlot(selectedSlot);
//    }

//    public void ClearSelectedSlot()
//    {
//        SaveSystem.DeleteSaveSlot(selectedSlot);
//        LoadWorldsForSlot(selectedSlot);
//    }
//}