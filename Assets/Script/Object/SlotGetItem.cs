using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotGetItem : MonoBehaviour, IDropHandler//implement drop handler
{
    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
    }
    //neu slot co item thi tra ve item
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
    public void OnDrop(PointerEventData eventData)
    {
        //Slot ko co item moi dc drop
        if(!Item)
        {
            RectTransform rectTransform = DragAndDropItem.itemIsDragging.GetComponent<RectTransform>();//use rect to work UI
            DragAndDropItem.itemIsDragging.transform.SetParent(transform, false);//convert transform follow parent(not world)
            rectTransform.anchoredPosition = Vector2.zero;//anchor -> UI and center
        }
    }
}
