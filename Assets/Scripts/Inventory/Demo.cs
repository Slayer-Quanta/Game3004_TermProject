using System.Runtime.CompilerServices;
using UnityEngine;

public class Demo : MonoBehaviour
{
public InventoryManager inventoryManager;
public Item[] itemsToPickup;

public void PickupItem(int id)
{
       bool result = inventoryManager.AddItem(itemsToPickup[id]);
       if (result == true)
       {
           Debug.Log("Item Added");
       }
       else
       {
           Debug.Log("Item not added!");
       }
}
}
