using UnityEngine;

public class InventoryDroppedItem : MonoBehaviour
{
    public InventoryItem inventoryItem;
    public void SetDroppedItem(InventoryItem inventoryItem)
    {
        this.inventoryItem = inventoryItem;
    }
}
