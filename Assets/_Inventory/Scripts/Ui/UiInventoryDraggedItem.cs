using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiInventoryDraggedItem : MonoBehaviour
{
    public InventoryItem item;
    public Image icon;
    public TextMeshProUGUI quantityText;
    public void SetDraggedItemDetails(InventoryItem item)
    {
        this.item = item;
        InventoryItemDetails itemDetails = InventoryManager.Singleton.GetItemDetails(item.ID);
        icon.sprite = itemDetails.icon;
        quantityText.text = item.quantity.ToString();
    }
}
