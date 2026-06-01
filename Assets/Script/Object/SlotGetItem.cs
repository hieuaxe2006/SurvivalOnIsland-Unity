using UnityEngine;
using UnityEngine.EventSystems;

public class SlotGetItem : MonoBehaviour, IDropHandler
{
    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
    }

    // Returns the child item if this slot has one
    public GameObject Item
    {
        get
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }
            return null;
        }
    }

    /// <summary>Handles dropping an item into this slot.</summary>
    public void OnDrop(PointerEventData eventData)
    {
        // Only allow drop if slot is empty
        if (!Item)
        {
            RectTransform rectTransform = DragAndDropItem.itemIsDragging.GetComponent<RectTransform>();
            DragAndDropItem.itemIsDragging.transform.SetParent(transform, false);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
