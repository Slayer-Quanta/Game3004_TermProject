//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class SaveSlotUI : MonoBehaviour
//{
//    [SerializeField] private TMP_Text slotNumberText;
//    [SerializeField] private TMP_Text playTimeText;
//    [SerializeField] private Button loadButton;
//    [SerializeField] private Button saveButton;

//    private int slotNumber;
//    private SaveSlotManager manager;

//    public void Setup(int slot, SaveSlotManager slotManager)
//    {
//        slotNumber = slot;
//        manager = slotManager;

//        slotNumberText.text = $"Slot {slot}";

//        SaveData data = SaveSystem.LoadGame(slot);
//        if (data != null)
//        {
//            playTimeText.text = $"Time Played: {FormatTime(data.timePlayed)}";
//            loadButton.interactable = true;
//        }
//        else
//        {
//            playTimeText.text = "Empty Slot";
//            loadButton.interactable = false;
//        }

//        saveButton.onClick.AddListener(() => manager.SelectSlot(slot));
//        loadButton.onClick.AddListener(() => manager.LoadFromSelectedSlot());
//    }

//    private string FormatTime(float timeInSeconds)
//    {
//        int hours = Mathf.FloorToInt(timeInSeconds / 3600);
//        int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
//        return $"{hours}h {minutes}m";
//    }
//}
