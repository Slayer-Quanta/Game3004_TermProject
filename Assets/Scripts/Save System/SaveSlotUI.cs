//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class SaveSlotUI : MonoBehaviour
//{
//    [SerializeField] private TMP_Text slotNumberText;
//    [SerializeField] private TMP_Text worldCountText;
//    [SerializeField] private Button slotButton;
//    [SerializeField] private Image backgroundImage;
//    [SerializeField] private Color normalColor = Color.white;
//    [SerializeField] private Color selectedColor = Color.cyan;

//    public int SlotId { get; private set; }
//    private SaveSlotManager manager;

//    public void Setup(int slot, SaveSlotManager slotManager, int worldCount)
//    {
//        SlotId = slot;
//        manager = slotManager;

//        slotNumberText.text = $"Slot {slot + 1}";
//        worldCountText.text = worldCount > 0 ? $"{worldCount} world(s)" : "Empty";

//        slotButton.onClick.AddListener(() => manager.SelectSlot(slot));
//    }

//    public void SetSelected(bool isSelected)
//    {
//        if (backgroundImage != null)
//        {
//            backgroundImage.color = isSelected ? selectedColor : normalColor;
//        }
//    }
//}