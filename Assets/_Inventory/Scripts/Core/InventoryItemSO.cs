using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Items", menuName = "Scriptable Object/Inventory/Item", order = 1)]
public class InventoryItemSO : ScriptableObject
{
    public List<InventoryItemDetails> details;
}
[System.Serializable]
public class InventoryItemDetails
{
    public string name;
    public Sprite icon;
    public GameObject prefab;
    public int maxStack = 0;
    public Vector2Int useRange;
    [TextArea] public string description;
    public InventoryItemDetailsExtra extra;
    [HideInInspector] public string ID;
}
[System.Serializable]
public class InventoryItemDetailsExtra
{
    public ItemType itemType;
    public int maxDurability;
    public int durabilityCostPerUse;
    public AudioClip useSound;
}

public enum ItemType
{
    None,
    Weapon,
    Armor,
    Food,
    Pickaxe,
    Block
}