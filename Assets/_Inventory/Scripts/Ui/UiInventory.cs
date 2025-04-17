using System;
using UnityEngine;

public class UiInventory : MonoBehaviour
{
    public UiInventoryItemSlot[] slots;
    public RectTransform inventoryPanel;
    [HideInInspector] public Inventory inventory { get; private set; }
    [field:SerializeField] public UiInventoryType uiInventoryType { get; private set; }
    private void Awake()
    {
        if (inventoryPanel == null)
        {
            gameObject.TryGetComponent<RectTransform>(out var rect);
            if(rect != null) { inventoryPanel = rect; }
            else { Debug.LogWarning("No Ui Panel Found!"); }
        }
    }
    public void SetUiInventory(Inventory inventory)
    {
        this.inventory = inventory;
        for (int i = 0; i < slots.Length; i++)
        {
            if(i == inventory.items.Length) { break; }
            slots[i].SetUiInventorySlot(i,this);
        }  
    }

    public enum UiInventoryType
    {
        None,
        PlayerInventory,
        PlayerHotbar,
        Chest
    }
}
