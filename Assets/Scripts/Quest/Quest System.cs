using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Core Quest System containing all quest classes and manager functionality
/// </summary>

// Quest Objective class - represents a single objective within a quest
[System.Serializable]
public class QuestObjective
{
    public string id;
    public string description;
    public int current;
    public int required;
    public bool completed;

    public QuestObjective(string id, string description, int required)
    {
        this.id = id;
        this.description = description;
        this.required = required;
        current = 0;
        completed = false;
    }

    public void UpdateProgress(int amount)
    {
        current += amount;
        if (current >= required && !completed)
        {
            completed = true;
        }
    }

    public float GetProgress()
    {
        return (float)current / required;
    }
}

// Quest class - represents a complete quest with multiple objectives
[System.Serializable]
public class Quest
{
    public string id;
    public string title;
    public string description;
    public List<QuestObjective> objectives;
    public bool active;
    public bool completed;
    public QuestType type;

    public enum QuestType { Tutorial, Main, Side }

    public Quest(string id, string title, string description, QuestType type)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.type = type;
        objectives = new List<QuestObjective>();
        active = false;
        completed = false;
    }

    public void AddObjective(QuestObjective objective)
    {
        objectives.Add(objective);
    }

    public void CheckCompletion()
    {
        if (objectives.Count == 0) return;

        foreach (var objective in objectives)
        {
            if (!objective.completed) return;
        }

        // All objectives are complete
        completed = true;
    }
}

// Quest Manager - Main controller for the quest system
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quests")]
    [SerializeField] private List<Quest> availableQuests = new List<Quest>();
    [SerializeField] private List<Quest> activeQuests = new List<Quest>();
    [SerializeField] private List<Quest> completedQuests = new List<Quest>();

    [Header("UI")]
    [SerializeField] private GameObject questNotificationPrefab;
    [SerializeField] private Transform notificationArea;
    [SerializeField] private GameObject questLogPanel;
    [SerializeField] private Transform questContainer;
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private GameObject questTrackerPanel;
    [SerializeField] private Transform trackerContainer;
    [SerializeField] private GameObject trackerEntryPrefab;
    [SerializeField] private Button questLogButton;

    [Header("Settings")]
    [SerializeField] private int maxTrackedQuests = 3;

    // Events
    public delegate void QuestEventHandler(Quest quest);
    public event QuestEventHandler OnQuestStarted;
    public event QuestEventHandler OnQuestCompleted;
    public event QuestEventHandler OnQuestUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Create default quests
        CreateDefaultQuests();

        // Hide quest log initially
        if (questLogPanel != null)
            questLogPanel.SetActive(false);

        // Set up button listener
        if (questLogButton != null)
            questLogButton.onClick.AddListener(ToggleQuestLog);

        // Start tutorial quests automatically
        StartTutorialQuests();
    }

    private void OnDestroy()
    {
        // Clean up button listener
        if (questLogButton != null)
            questLogButton.onClick.RemoveListener(ToggleQuestLog);
    }

    private void CreateDefaultQuests()
    {
        // Tutorial: Movement
        Quest moveQuest = new Quest("TUT_MOVE", "First Steps", "Learn how to move around in the world.", Quest.QuestType.Tutorial);
        moveQuest.AddObjective(new QuestObjective("move_steps", "Take 20 steps", 20));
        moveQuest.AddObjective(new QuestObjective("jump_times", "Jump 5 times", 5));
        availableQuests.Add(moveQuest);

        // Tutorial: Building
        Quest buildQuest = new Quest("TUT_BUILD", "Block Basics", "Learn how to interact with blocks.", Quest.QuestType.Tutorial);
        buildQuest.AddObjective(new QuestObjective("break_blocks", "Break 5 blocks", 5));
        buildQuest.AddObjective(new QuestObjective("place_blocks", "Place 5 blocks", 5));
        availableQuests.Add(buildQuest);

        // Main Quest: Exploration
        Quest exploreQuest = new Quest("MAIN_EXPLORE", "Explorer", "Discover new areas of the world.", Quest.QuestType.Main);
        exploreQuest.AddObjective(new QuestObjective("visit_chunks", "Discover 5 new chunks", 5));
        availableQuests.Add(exploreQuest);

        // Side Quest: Combat
        Quest combatQuest = new Quest("SIDE_COMBAT", "Monster Hunter", "Defeat enemies in the world.", Quest.QuestType.Side);
        combatQuest.AddObjective(new QuestObjective("defeat_enemies", "Defeat 3 enemies", 3));
        availableQuests.Add(combatQuest);
    }

    private void StartTutorialQuests()
    {
        foreach (Quest quest in availableQuests)
        {
            if (quest.type == Quest.QuestType.Tutorial)
            {
                StartQuest(quest.id);
            }
        }
    }

    public void StartQuest(string questId)
    {
        Quest quest = availableQuests.Find(q => q.id == questId);
        if (quest != null)
        {
            // Move quest from available to active
            quest.active = true;
            activeQuests.Add(quest);
            availableQuests.Remove(quest);

            // Show notification
            ShowNotification($"New Quest: {quest.title}");

            // Update UI
            UpdateQuestLog();
            UpdateQuestTracker();

            // Fire event
            OnQuestStarted?.Invoke(quest);
        }
    }

    public void UpdateObjective(string questId, string objectiveId, int amount = 1)
    {
        Quest quest = activeQuests.Find(q => q.id == questId);
        if (quest != null)
        {
            QuestObjective objective = quest.objectives.Find(o => o.id == objectiveId);
            if (objective != null && !objective.completed)
            {
                objective.UpdateProgress(amount);

                // Check if objective completed
                if (objective.completed)
                {
                    ShowNotification($"Objective Completed: {objective.description}");
                }

                // Check if quest completed
                quest.CheckCompletion();
                if (quest.completed)
                {
                    CompleteQuest(quest);
                }

                // Update UI
                UpdateQuestLog();
                UpdateQuestTracker();

                // Fire event
                OnQuestUpdated?.Invoke(quest);
            }
        }
    }

    private void CompleteQuest(Quest quest)
    {
        // Move quest from active to completed
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        // Show notification
        ShowNotification($"Quest Completed: {quest.title}");

        // Update UI
        UpdateQuestLog();
        UpdateQuestTracker();

        // Fire event
        OnQuestCompleted?.Invoke(quest);
    }

    // UI Methods
    public void ToggleQuestLog()
    {
        if (questLogPanel != null)
        {
            questLogPanel.SetActive(!questLogPanel.activeSelf);

            if (questLogPanel.activeSelf)
            {
                UpdateQuestLog();
            }
        }
    }

    public void UpdateQuestLog(Quest.QuestType? filterType = null)
    {
        if (questContainer == null || questEntryPrefab == null) return;

        // Clear existing entries
        foreach (Transform child in questContainer)
        {
            Destroy(child.gameObject);
        }

        // Add active quests (with optional filtering)
        foreach (Quest quest in activeQuests)
        {
            if (filterType == null || quest.type == filterType)
            {
                GameObject entry = Instantiate(questEntryPrefab, questContainer);
                SetupQuestEntry(entry, quest);
            }
        }
    }

    public void UpdateQuestTracker()
    {
        if (trackerContainer == null || trackerEntryPrefab == null) return;

        // Clear existing entries
        foreach (Transform child in trackerContainer)
        {
            Destroy(child.gameObject);
        }

        // Show only the first few active quests (up to maxTrackedQuests)
        for (int i = 0; i < Mathf.Min(activeQuests.Count, maxTrackedQuests); i++)
        {
            GameObject entry = Instantiate(trackerEntryPrefab, trackerContainer);
            SetupTrackerEntry(entry, activeQuests[i]);
        }
    }

    private void SetupQuestEntry(GameObject entryObject, Quest quest)
    {
        // Find UI components
        TextMeshProUGUI titleText = entryObject.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = entryObject.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        Transform objectivesParent = entryObject.transform.Find("Objectives");

        // Set quest info
        if (titleText != null) titleText.text = quest.title;
        if (descriptionText != null) descriptionText.text = quest.description;

        // Set objectives
        if (objectivesParent != null)
        {
            // Find objective text prefab
            GameObject objectivePrefab = objectivesParent.childCount > 0 ? objectivesParent.GetChild(0).gameObject : null;

            if (objectivePrefab != null)
            {
                // Clear existing objective texts (except the template)
                for (int i = 1; i < objectivesParent.childCount; i++)
                {
                    Destroy(objectivesParent.GetChild(i).gameObject);
                }

                // Hide the template
                objectivePrefab.SetActive(false);

                // Add objective texts
                foreach (QuestObjective objective in quest.objectives)
                {
                    GameObject obj = Instantiate(objectivePrefab, objectivesParent);
                    obj.SetActive(true);

                    // Set text
                    TextMeshProUGUI objText = obj.GetComponent<TextMeshProUGUI>();
                    if (objText != null)
                    {
                        string checkmark = objective.completed ? "✓ " : "□ ";
                        objText.text = $"{checkmark}{objective.description} ({objective.current}/{objective.required})";
                    }
                }
            }
        }
    }

    private void SetupTrackerEntry(GameObject entryObject, Quest quest)
    {
        // Find UI components
        TextMeshProUGUI titleText = entryObject.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        Transform objectivesParent = entryObject.transform.Find("Objectives");

        // Set quest info
        if (titleText != null) titleText.text = quest.title;

        // Set objectives
        if (objectivesParent != null)
        {
            // Find objective text prefab
            GameObject objectivePrefab = objectivesParent.childCount > 0 ? objectivesParent.GetChild(0).gameObject : null;

            if (objectivePrefab != null)
            {
                // Clear existing objective texts (except the template)
                for (int i = 1; i < objectivesParent.childCount; i++)
                {
                    Destroy(objectivesParent.GetChild(i).gameObject);
                }

                // Hide the template
                objectivePrefab.SetActive(false);

                // Add objective texts (only non-completed objectives)
                foreach (QuestObjective objective in quest.objectives)
                {
                    if (!objective.completed)
                    {
                        GameObject obj = Instantiate(objectivePrefab, objectivesParent);
                        obj.SetActive(true);

                        // Set text
                        TextMeshProUGUI objText = obj.GetComponent<TextMeshProUGUI>();
                        if (objText != null)
                        {
                            objText.text = $"{objective.description} ({objective.current}/{objective.required})";
                        }
                    }
                }
            }
        }
    }

    private void ShowNotification(string message)
    {
        if (notificationArea == null || questNotificationPrefab == null) return;

        GameObject notification = Instantiate(questNotificationPrefab, notificationArea);

        // Find text component
        TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();
        if (notificationText != null)
        {
            notificationText.text = message;
        }

        // Auto-destroy after delay
        Destroy(notification, 3f);
    }

    // Helper methods
    public Quest GetQuestByID(string questId)
    {
        // Search in all quest lists
        Quest quest = availableQuests.Find(q => q.id == questId);
        if (quest != null) return quest;

        quest = activeQuests.Find(q => q.id == questId);
        if (quest != null) return quest;

        quest = completedQuests.Find(q => q.id == questId);
        return quest;
    }

    public List<Quest> GetActiveQuests()
    {
        return activeQuests;
    }
}