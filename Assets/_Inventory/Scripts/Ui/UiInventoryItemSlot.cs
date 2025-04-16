using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiInventoryItemSlot : MonoBehaviour,IDragHandler,IPointerClickHandler
{
    [Header("Details")]
    public Image iconImage;
    public TextMeshProUGUI quantityText;
    public Image durabilityImage;
    public Image highLightImage;
    [HideInInspector] public UiInventory uiInventory { get; private set; }
    [HideInInspector] public int index { get; private set; }

    public void SetUiInventorySlot(int index, UiInventory uiInventory)
    {
        this.index = index;
        this.uiInventory = uiInventory;
        var inventoryItem = uiInventory.inventory.items[index];
        if (inventoryItem.quantity == 0 || inventoryItem.ID == "" || inventoryItem.ID == null)
        {
            iconImage.gameObject.SetActive(false);
            quantityText.gameObject.SetActive(false);
            durabilityImage.gameObject.SetActive(false);
        }
        else
        {
            InventoryItemDetails itemDetails = InventoryManager.Singleton.GetItemDetails(inventoryItem.ID);
            iconImage.sprite = itemDetails.icon;
            quantityText.text = inventoryItem.quantity.ToString();

            iconImage.gameObject.SetActive(true);
            quantityText.gameObject.SetActive(true);

            if (itemDetails.extra.maxDurability > 0)
            {
                float durabilityBarFillAmount = (float)inventoryItem.durability / (float)itemDetails.extra.maxDurability;
                durabilityImage.fillAmount = durabilityBarFillAmount;
                durabilityImage.gameObject.SetActive(true);
            }
            else
            {
                durabilityImage.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject != null &&
            uiInventory != null && uiInventory.inventory != null)
        {
            if (uiInventory.uiInventoryType == UiInventory.UiInventoryType.PlayerHotbar)
            {
                InventoryManager.Singleton.playerInventory.SelectItem(index);
                return;
            }

            if (InventoryManager.Singleton.currentDraggedItem == null)
            {
                InventoryManager.Singleton.TakeInventoryItem(this, uiInventory.inventory.items[index].quantity);
            }
            else
            {
                InventoryManager.Singleton.PutInventoryItem(this,InventoryManager.Singleton.currentDraggedItem.item.quantity);
            }
        }
        else
        {
            InventoryManager.Singleton.DropInventoryItem();
        }
    }
    public void SelectSlot(int index)
    {
        highLightImage.gameObject.SetActive(index == this.index);
    }
    public void OnDrag(PointerEventData eventData) { }
}

