using System;
using UnityEngine;

public class PlayerInventory : Inventory 
{
    [Header("Player Inventory Settings")]
    public bool showHotbarOnStart = true;
    public bool useMouseScrollToSelectItem = true;
    public KeyCode playerInventoryOpenKey = KeyCode.Tab;
    public UiInventory playerInventoryUi;
    public UiInventory playerItemHotbarUi;
    [HideInInspector] public int selectedItemIndex;
    public InventoryItem selectedItem;
    public InventoryItemDetails selectedItemDetails;
    private void Awake()
    {
        selectedItemIndex = -1;
        uiInventoryType = UiInventory.UiInventoryType.PlayerInventory;
    }
    private void Start()
    {
        if (showHotbarOnStart)
        {
            ShowPlayerHotbar(true);
            InventoryManager.Singleton.currentlyOpenedInventoryType = UiInventory.UiInventoryType.PlayerHotbar;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UseSelectedItem();
        }
    }
    public void ShowPlayerInventory(bool toActive)
    {
        if (playerInventoryUi != null && playerInventoryUi.slots.Length > 0)
        {
            playerInventoryUi.SetUiInventory(this);
            playerInventoryUi.gameObject.SetActive(toActive);
        }
    }
    public void ShowPlayerHotbar(bool toActive)
    {
        if (playerItemHotbarUi != null && playerItemHotbarUi.slots.Length > 0)
        {
            playerItemHotbarUi.SetUiInventory(this);
            playerItemHotbarUi.gameObject.SetActive(toActive);
        }
    }
    public void SelectNextItem() => SelectItem(selectedItemIndex++);
    public void SelectPreviousItem() => SelectItem(selectedItemIndex--);
 
    public void SelectItem(int index)
    {
        if (index != playerItemHotbarUi.slots.Length && items[index] == selectedItem)
        {
            selectedItem = null;
            selectedItemIndex = -1;
            selectedItemDetails = null;
        }
        else
        {
            if (index < 0)
            {
                selectedItemIndex = playerItemHotbarUi.slots.Length - 1;
            }
            else if (index >= playerItemHotbarUi.slots.Length)
            {
                selectedItemIndex = 0;
            }
            else
            {
                selectedItemIndex = index;
            }
            selectedItem = items[selectedItemIndex];
            selectedItemDetails = InventoryManager.Singleton.GetItemDetails(selectedItem.ID);
        }
        foreach (var slot in playerItemHotbarUi.slots)
        {
            slot.SelectSlot(selectedItemIndex);
        }
    }
    public void UseSelectedItem()
    {
        if (selectedItem != null &&
            selectedItem.ID != null &&
            selectedItem.ID != "" &&
            selectedItem.quantity > 0)
        {
            if (selectedItemDetails.extra.maxDurability > 0)
            {
                selectedItem.durability -= selectedItemDetails.extra.durabilityCostPerUse;
                if (selectedItem.durability > 0)
                {
                    playerInventoryUi.SetUiInventory(this);
                    playerItemHotbarUi.SetUiInventory(this);
                    return;
                }
            }
            InventoryManager.Singleton.RemoveItemFromInventory(this, selectedItemIndex, 1);
        }
        if (selectedItem.quantity <= 0)
        {
            SelectItem();
        }
        playerInventoryUi.SetUiInventory(this);
        playerItemHotbarUi.SetUiInventory(this);
    }
}
