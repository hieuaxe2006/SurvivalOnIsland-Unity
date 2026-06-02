using UnityEngine;
using UnityEngine.EventSystems;

// Implements drag/drop and right-click equip handlers
public class DragAndDropItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Transform dragLayer;
    public static GameObject itemIsDragging;

    private Vector2 originPos;
    private Transform originSlot;
    private InventoryItem inventoryItem;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryItem = GetComponent<InventoryItem>();
        // Find the top UI layer for dragging
        dragLayer = GameObject.Find("dragLayer")?.transform;
    }

    /// <summary>Equips the item on right-click.</summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayClick();
            }
            if (inventoryItem != null && inventoryItem.itemData != null)
            {
                if (EquipManager.Instance != null)
                {
                    EquipManager.Instance.Equip(inventoryItem.itemData);
                }
            }
        }
    }

    /// <summary>Begins dragging the item.</summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
        // Save original position and parent slot
        originPos = rectTransform.anchoredPosition;
        originSlot = transform.parent;

        // Move to drag layer so it renders on top
        transform.SetParent(dragLayer, true);
        itemIsDragging = gameObject;
    }

    /// <summary>Moves the item with the mouse during drag.</summary>
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    /// <summary>Ends the drag and snaps back if not dropped on a valid slot.</summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        itemIsDragging = null;

        // Return to original slot if still on drag layer
        if (transform.parent == dragLayer)
        {
            transform.SetParent(originSlot, false);
            rectTransform.anchoredPosition = originPos;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}
