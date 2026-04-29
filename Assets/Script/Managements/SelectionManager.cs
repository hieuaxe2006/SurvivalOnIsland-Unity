using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SelectionManager : MonoBehaviour
{
    [SerializeField] private GameObject infoUI;
    private TMP_Text infoText;
    // Start is called before the first frame update
    void Start()
    {
        //get component
        infoText = infoUI.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        //tao raycast
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//lay ray tu screen -> mouse position
        //tim object co collider
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;//if found object->save its transform
            //check co script interactableObject ko
            if(selectionTransform.GetComponent<InteractableObject>())
            {
                infoText.text = selectionTransform.GetComponent<InteractableObject>().getObjectName();//get name object to show
                infoUI.SetActive(true);//show
            }
            else infoUI.SetActive(false);//if no script no show
        }
    }
}
