//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class SaveSlotManager : MonoBehaviour
//{
//    [SerializeField] private Transform slotContainer; // Parent UI container for slots
//    [SerializeField] private GameObject saveSlotPrefab; // The save slot UI prefab
//    private SaveSlotUI[] saveSlots; // Holds UI references

//    private float currentPlayTime = 0f;
//    private int selectedSlot = 1; // Default slot

//    private void Start()
//    {
//        LoadSaveSlots();
//    }

//    private void Update()
//    {
//        currentPlayTime += Time.deltaTime; // Track playtime every frame
//    }

//    public void LoadSaveSlots()
//    {
//        foreach (Transform child in slotContainer)
//        {
//            Destroy(child.gameObject); // Clear existing slots
//        }

//        int maxSlots = 3; // Set the max number of slots
//        saveSlots = new SaveSlotUI[maxSlots];

//        for (int i = 0; i < maxSlots; i++)
//        {
//            int slotNumber = i + 1;
//            GameObject newSlot = Instantiate(saveSlotPrefab, slotContainer);
//            SaveSlotUI slotUI = newSlot.GetComponent<SaveSlotUI>();
//            slotUI.Setup(slotNumber, this);
//            saveSlots[i] = slotUI;
//        }
//    }

//    public void SelectSlot(int slot)
//    {
//        selectedSlot = slot;
//        Debug.Log($"Selected Save Slot: {selectedSlot}");
//    }

//    public void SaveToSelectedSlot()
//    {
//        SaveSystem.SaveGame(selectedSlot, currentPlayTime);
//        LoadSaveSlots(); // Refresh UI
//    }

//    public void LoadFromSelectedSlot()
//    {
//        SaveSystem.LoadGame(selectedSlot);
//    }
//}
