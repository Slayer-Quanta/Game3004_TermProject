using UnityEngine;
[RequireComponent(typeof(Collider))]
public class InventoryDroppedItem : MonoBehaviour
{
    public InventoryItem inventoryItem;
    Camera _mainCamera;
    private void Start()
    {
        _mainCamera = Camera.main;
    }
    public void SetDroppedItem(InventoryItem inventoryItem)
    {
        this.inventoryItem = inventoryItem;
        //GetComponent<SpriteRenderer>().sprite = InventoryManager.Singleton.GetItemDetails(inventoryItem.ID).icon;
    }
    private void LateUpdate()
    {
        //Vector3 camPosition = _mainCamera.transform.position;
        //camPosition.y = transform.position.y;
        //transform.LookAt(camPosition);
        //transform.Rotate(0, 180, 0);
    }
}
