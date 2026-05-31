using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;
//implement 3 drag handler and 1 click handler
public class DragAndDropItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Canvas canvas;//scale when drag
    [SerializeField] private RectTransform rectTransform;//transform for UI
    [SerializeField] private CanvasGroup canvasGroup;//used for opacity and on/off raycast
    [SerializeField] private Transform dragLayer;//always show on top
    public static GameObject itemIsDragging;//static allow other scripts know this

    private Vector2 originPos;
    private Transform originSlot;
    private InventoryItem inventoryItem;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryItem = GetComponent<InventoryItem>();
        //find dragLayer(top ui layer) in scene
        dragLayer = GameObject.Find("dragLayer")?.transform;
    }
    //func call equip func if m2 click
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.5f;//opacity = 50%
        canvasGroup.blocksRaycasts = false;//ko chan raycast->lay ray toi slot o layer sau
        //save origin pos and slot
        originPos = rectTransform.anchoredPosition;
        originSlot = transform.parent;

        transform.SetParent(dragLayer, true);//dua len dragLayer(top) de ko bi che
        itemIsDragging = gameObject;//danh dau item is dragging
    }
    public void OnDrag(PointerEventData eventData)//PointerEventData la all action cua mouse
    {
        rectTransform.anchoredPosition += eventData.delta/canvas.scaleFactor;//anchoredPos work with UI, eventData.delta :do di chuyen chuot
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        itemIsDragging = null;

        //if parent is dragLayer
        if (transform.parent == dragLayer)
        {
            //return back pos
            transform.SetParent(originSlot, false);
            rectTransform.anchoredPosition = originPos;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}
