using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string ID;
    public int quantity;
    public int durability;
    public InventoryItem() { }
    public InventoryItem(InventoryItem item)
    {
        this.ID = item.ID;
        this.quantity = item.quantity;
        this.durability = item.durability;
    }
    public InventoryItem(string ID, int quatity, int durability)
    {
        this.ID = ID;
        this.quantity = quatity;
        this.durability = durability;
    }
}