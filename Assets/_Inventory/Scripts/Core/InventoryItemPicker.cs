using UnityEngine;
[RequireComponent(typeof(Inventory))]
public class InventoryItemPicker : MonoBehaviour
{
    public AudioClip itemPickupAudio;
    public Inventory inventoryToAddItem { get; private set; }
    private void Awake()
    {
        inventoryToAddItem = GetComponent<Inventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<InventoryDroppedItem>(out var droppedItem))
        {
            InventoryManager.Singleton.AddDroppedItem(droppedItem, this);
        }
    }
}
