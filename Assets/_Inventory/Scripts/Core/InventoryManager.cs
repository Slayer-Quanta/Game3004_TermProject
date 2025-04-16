using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerInventory))]
public class InventoryManager : MonoBehaviour
{
    public UiInventory.UiInventoryType currentlyOpenedInventoryType = UiInventory.UiInventoryType.None;
    [Header("Inventory Settings")]
    public UiInventoryDraggedItem uiDraggedItemPrefab;
    public Transform itemDropTransform;
    public InventoryItemSO inventoryItemSO;
    public InventoriesUi[] inventoryUis;

    [HideInInspector] public PlayerInventory playerInventory { get; protected set; }
    public Inventory currentlyOpenedInventory { get; protected set; }

    private Dictionary<string, InventoryItemDetails> inventoryItemDetailsDictionary = new();
    public static InventoryManager Singleton { get; private set; }

    protected Canvas canvas;
    private EventSystem eventSystem;
    public UiInventoryDraggedItem currentDraggedItem { get; protected set; }

    [System.Serializable]
    public struct InventoriesUi
    {
        [HideInInspector] public string name;
        public UiInventory.UiInventoryType uiInventoryType;
        public UiInventory inventoryUi;
        public UiInventory playerInventoryUi;
    }
    private void OnValidate()
    {
        if(inventoryUis == null) { return; }
        for (int i = 0; i < inventoryUis.Length; i++)
        {
            inventoryUis[i].name = inventoryUis[i].uiInventoryType.ToString();
        }
    }

    protected virtual void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        eventSystem = EventSystem.current;
        playerInventory = GetComponent<PlayerInventory>();
        canvas = FindAnyObjectByType<Canvas>();
        InitializeInventoryItemDictionary();
    }

    private void InitializeInventoryItemDictionary()
    {
        for (int d = 0; d < inventoryItemSO.details.Count; d++)
        {
            var item = inventoryItemSO.details[d];
            string itemKey = "#" + item.name;
            if (inventoryItemDetailsDictionary.ContainsKey(itemKey))
            {
                Debug.LogError("Item With Name '" + item.name + "' Already Exist! Can't Have Multiple Items With Same Name!");
            }
            else
            {
                inventoryItemDetailsDictionary[itemKey] = item;
            }
        }
    }

    public InventoryItemDetails GetItemDetails(string ID)
    {
        if(ID == null || ID == "") { return null; }
        if (inventoryItemDetailsDictionary.TryGetValue(ID, out var itemDetails))
        {
            return itemDetails;
        }
        Debug.LogError($"Item With ID: {ID} not found in inventory!");
        return null;
    }

    private void Update()
    {
        UpdateDraggedItemPosition();
        HandleMouseScroll();

        if (Input.GetKeyDown(playerInventory.playerInventoryOpenKey))
        {
            TogglePlayerInventory();
        }
        if (Input.GetMouseButtonDown(0))
        {
            HandleItemDrop();
        }
    }

    private void HandleItemDrop()
    {
        if (currentDraggedItem != null)
        {
            var objectUnderPOinter = PointerOverUIObject(Input.mousePosition);
            if (objectUnderPOinter == null ||
                objectUnderPOinter.GetComponent<UiInventoryItemSlot>() == null)
            {
                DropInventoryItem();
            }
        }
    }

    public void HandleMouseScroll()
    {
        int mouseScrollDelta = (int)Input.mouseScrollDelta.y;
        if (mouseScrollDelta != 0)
        {
            if (playerInventory.useMouseScrollToSelectItem && currentlyOpenedInventory == null)
            {
                playerInventory.SelectItem(playerInventory.selectedItemIndex + mouseScrollDelta);
            }
        }
    }

    private GameObject PointerOverUIObject(Vector2 touchPosition)
    {
        PointerEventData eventData = new PointerEventData(eventSystem) { position = touchPosition };
        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(eventData, results);
        if (results.Count > 0)
        {
            return results[0].gameObject;
        }
        return null;
    }
    private void UpdateDraggedItemPosition()
    {
        if (currentDraggedItem != null)
        {
            currentDraggedItem.transform.position = Input.mousePosition;
        }
    }

    private void TogglePlayerInventory()
    {
        if (currentlyOpenedInventory == null)
        {
            if (currentlyOpenedInventoryType == UiInventory.UiInventoryType.PlayerHotbar)
            {
                Cursor.lockState = CursorLockMode.None;
                playerInventory.ShowPlayerInventory(true);
                playerInventory.ShowPlayerHotbar(false);
                currentlyOpenedInventoryType = UiInventory.UiInventoryType.PlayerInventory;
            }
            else
            {
                currentlyOpenedInventoryType = UiInventory.UiInventoryType.PlayerHotbar;
                playerInventory.ShowPlayerInventory(false);
                playerInventory.ShowPlayerHotbar(true);
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        else
        {
            CloseAllInventory();
            currentlyOpenedInventoryType = UiInventory.UiInventoryType.PlayerInventory;
            playerInventory.ShowPlayerInventory(true);
            playerInventory.ShowPlayerHotbar(false);

        }
    }

    public virtual void OpenInventory(Inventory inventory)
    {
        bool closeInventory = inventory == null || inventory == currentlyOpenedInventory;
        if (closeInventory)
        {
            CloseAllInventory();
            playerInventory.ShowPlayerHotbar(true);
            currentlyOpenedInventoryType = UiInventory.UiInventoryType.PlayerHotbar;
        }
        else
        {
            currentlyOpenedInventory = inventory;
            currentlyOpenedInventoryType = inventory.uiInventoryType;

            playerInventory.ShowPlayerHotbar(false);
            playerInventory.ShowPlayerInventory(false);
            foreach (var inventoryUi in inventoryUis)
            {
                bool isInventoryUiType = inventoryUi.uiInventoryType == inventory.uiInventoryType;
                if (isInventoryUiType)
                {
                    inventoryUi.inventoryUi.SetUiInventory(currentlyOpenedInventory);
                    inventoryUi.playerInventoryUi.SetUiInventory(playerInventory);

                    inventoryUi.playerInventoryUi.gameObject.SetActive(true);
                    inventoryUi.inventoryUi.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }
    private void CloseAllInventory()
    {
        currentlyOpenedInventory = null;
        currentlyOpenedInventoryType = UiInventory.UiInventoryType.None;
        foreach (var inventoryUi in inventoryUis)
        {
            inventoryUi.playerInventoryUi.gameObject.SetActive(false);
            inventoryUi.inventoryUi.gameObject.SetActive(false);
        }
    }

    public virtual void TakeInventoryItem(UiInventoryItemSlot inventorySlot, int quantity)
    {
        var inventory = inventorySlot.uiInventory.inventory;
        var item = inventory.items[inventorySlot.index];
        if(item.quantity == 0|| item.ID == null || item.ID == "") { item = new(); return; }

        InventoryItem itemToRemove = new(item.ID, quantity, item.durability);
        currentDraggedItem = Instantiate(uiDraggedItemPrefab, canvas.transform);
        currentDraggedItem.SetDraggedItemDetails(itemToRemove);

        RemoveItemFromInventory(inventory, inventorySlot.index, quantity);
        inventorySlot.uiInventory.SetUiInventory(inventory);
    }

    public virtual void PutInventoryItem(UiInventoryItemSlot toInventorySlot, int quantityToPut)
    {
        if (currentDraggedItem == null) return;

        var itemDetails = GetItemDetails(currentDraggedItem.item.ID);
        InventoryItem toSlotItem = toInventorySlot.uiInventory.inventory.items[toInventorySlot.index];

        if (itemDetails == null)
        {
            return;
        }

        if (toSlotItem.ID == null ||
            toSlotItem.ID == "" ||
            toSlotItem.quantity == 0)
        {
            InventoryItem itemToPut = new(currentDraggedItem.item);
            itemToPut.quantity = quantityToPut;
            toInventorySlot.uiInventory.inventory.items[toInventorySlot.index] = itemToPut;
            currentDraggedItem.item.quantity -= quantityToPut;
            if (currentDraggedItem.item.quantity > 0)
            {
                currentDraggedItem.SetDraggedItemDetails(currentDraggedItem.item);
            }
            else
            {
                Destroy(currentDraggedItem.gameObject);
            }
        }
        else if (toSlotItem.ID == currentDraggedItem.item.ID)
        {
            if (toSlotItem.quantity == itemDetails.maxStack) { return; }
            int quantityAfterAdd = toSlotItem.quantity + quantityToPut;
            if (quantityAfterAdd <= itemDetails.maxStack)
            {
                toInventorySlot.uiInventory.inventory.items[toInventorySlot.index].quantity += quantityToPut;
                currentDraggedItem.item.quantity -= quantityToPut;
                if (currentDraggedItem.item.quantity > 0)
                {
                    currentDraggedItem.SetDraggedItemDetails(currentDraggedItem.item);
                }
                else
                {
                    Destroy(currentDraggedItem.gameObject);
                }
            }
            else
            {
                toInventorySlot.uiInventory.inventory.items[toInventorySlot.index].quantity = itemDetails.maxStack;

                InventoryItem currentItemAfterAdd = new(currentDraggedItem.item);
                currentItemAfterAdd.quantity = quantityAfterAdd - itemDetails.maxStack;
                currentDraggedItem.SetDraggedItemDetails(currentItemAfterAdd);
            }
        }
        else
        {
            InventoryItem itemToPut = new(currentDraggedItem.item);
            currentDraggedItem.SetDraggedItemDetails(toInventorySlot.uiInventory.inventory.items[toInventorySlot.index]);
            toInventorySlot.uiInventory.inventory.items[toInventorySlot.index] = itemToPut;
        }
        toInventorySlot.uiInventory.SetUiInventory(toInventorySlot.uiInventory.inventory);
    }

    public virtual void RemoveItemFromInventory(Inventory inventory, int index, int quantity)
    {
        inventory.items[index].quantity -= quantity;
        if (inventory.items[index].quantity <= 0)
        {
            inventory.items[index] = new();
        }
    }

    public virtual void DropInventoryItem()
    {
        if (currentDraggedItem == null) return;
        var itemPrefab = GetItemDetails(currentDraggedItem.item.ID)?.prefab;
        if (itemPrefab != null)
        {
            Debug.Log("Drop Item");
            if (itemDropTransform == null)
            {
                itemDropTransform = transform;
            }
            var dropItem = Instantiate(itemPrefab, itemDropTransform.position, Quaternion.identity, itemDropTransform);
            if (dropItem.GetComponent<InventoryDroppedItem>() == null)
            {
                dropItem.AddComponent<InventoryDroppedItem>();
            }
            dropItem.GetComponent<InventoryDroppedItem>().SetDroppedItem(currentDraggedItem.item);
        }
        else
        {
            Debug.LogError("No Item Prefab Found To Drop In The Items ScriptableObject");
        }

        Destroy(currentDraggedItem.gameObject);
    }

    public virtual void AddDroppedItem(InventoryDroppedItem droppedItem)
    {
        Debug.Log("Try Add Droped Item");
        var droppedItemData = droppedItem.inventoryItem;
        var inventoryItems = playerInventory.items;
        var itemDetails = GetItemDetails(droppedItemData.ID);

        if (itemDetails == null) return;

        int remainingQuantity = droppedItemData.quantity;

        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (string.IsNullOrEmpty(inventoryItems[i].ID) || inventoryItems[i].ID == droppedItemData.ID)
            {
                int availableSpace = itemDetails.maxStack - inventoryItems[i].quantity;

                if (remainingQuantity <= availableSpace)
                {
                    inventoryItems[i].quantity += remainingQuantity;
                    inventoryItems[i].ID = droppedItemData.ID;
                    Destroy(droppedItem.gameObject);
                    playerInventory.playerInventoryUi.SetUiInventory(playerInventory);
                    playerInventory.playerItemHotbarUi.SetUiInventory(playerInventory);
                    return;
                }
                else
                {
                    inventoryItems[i].ID = droppedItemData.ID;
                    inventoryItems[i].quantity = itemDetails.maxStack;
                    remainingQuantity -= availableSpace;
                }
            }
        }
        playerInventory.playerInventoryUi.SetUiInventory(playerInventory);
        playerInventory.playerItemHotbarUi.SetUiInventory(playerInventory);
        droppedItem.SetDroppedItem(new InventoryItem(droppedItemData.ID, remainingQuantity, droppedItemData.durability));
    }
}