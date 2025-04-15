//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;
//using System;

//public class WorldEntryUI : MonoBehaviour
//{
//    [SerializeField] private TMP_Text worldNameText;
//    [SerializeField] private TMP_Text worldInfoText;
//    [SerializeField] private TMP_Text lastPlayedText;
//    [SerializeField] private Button selectButton;
//    [SerializeField] private Button deleteButton;
//    [SerializeField] private Image backgroundImage;
//    [SerializeField] private Color normalColor = Color.white;
//    [SerializeField] private Color selectedColor = Color.cyan;

//    public string WorldId { get; private set; }
//    private SaveSlotManager manager;

//    public void Setup(WorldSave world, SaveSlotManager slotManager)
//    {
//        WorldId = world.worldId;
//        manager = slotManager;

//        worldNameText.text = world.worldName;

//        // Format play time
//        string playTimeStr = FormatPlayTime(world.playTime);
//        worldInfoText.text = $"Seed: {world.worldSeed}\nPlayed: {playTimeStr}";

//        // Parse last played date
//        DateTime lastPlayed;
//        if (DateTime.TryParse(world.lastPlayedDate, out lastPlayed))
//        {
//            lastPlayedText.text = $"Last played: {lastPlayed.ToString("MMM d, yyyy")}";
//        }
//        else
//        {
//            lastPlayedText.text = "New world";
//        }

//        selectButton.onClick.AddListener(() => {
//            manager.SelectWorld(world.worldId);
//        });

//        deleteButton.onClick.AddListener(() => {
//            manager.DeleteSelectedWorld();
//        });

//        // Double-click/tap to start
//        selectButton.onClick.AddListener(() => {
//            if (WorldId == manager.selectedWorldId)
//            {
//                manager.StartSelectedWorld();
//            }
//        });
//    }

//    public void SetSelected(bool isSelected)
//    {
//        if (backgroundImage != null)
//        {
//            backgroundImage.color = isSelected ? selectedColor : normalColor;
//        }
//    }

//    private string FormatPlayTime(float timeInSeconds)
//    {
//        TimeSpan time = TimeSpan.FromSeconds(timeInSeconds);

//        if (time.TotalHours >= 1)
//        {
//            return $"{time.Hours}h {time.Minutes}m";
//        }
//        else if (time.TotalMinutes >= 1)
//        {
//            return $"{time.Minutes}m {time.Seconds}s";
//        }
//        else
//        {
//            return $"{time.Seconds}s";
//        }
//    }
//}