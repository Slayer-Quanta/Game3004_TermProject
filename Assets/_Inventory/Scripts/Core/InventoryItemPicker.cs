using UnityEngine;
[RequireComponent(typeof(Collider))]
public class InventoryItemPicker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<InventoryDroppedItem>(out var droppedItem))
        {
            InventoryManager.Singleton.AddDroppedItem(droppedItem);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<InventoryDroppedItem>(out var droppedItem))
        {
            InventoryManager.Singleton.AddDroppedItem(droppedItem);
        }
    }
}
