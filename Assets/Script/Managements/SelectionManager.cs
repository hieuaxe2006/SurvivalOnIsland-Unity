using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SelectionManager : MonoBehaviour
{
    public static SelectionManager instance;//change to singleton for more accessable

    [SerializeField] private GameObject infoUI;
    private TMP_Text infoText;
    [Header("Cooldown")]
    [SerializeField] private float atackRate = 1f;//cooldown
    private float nextAttack = 0f;

    private InteractableObject ObjectScriptedCurrent;
    private PlayerMovement playerMovement;
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //get component
        infoText = infoUI.GetComponent<TMP_Text>();
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        //tao raycast
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//lay ray tu screen -> mouse position
        //tim object co collider
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))//if hit something
        {
            var selectionTransform = hit.transform;//save its transform
            InteractableObject scriptInteract = selectionTransform.GetComponent<InteractableObject>();//script interact
            //check co script va in range ko
            if (scriptInteract && scriptInteract.isInRange)
            {
                ObjectScriptedCurrent = scriptInteract;//avoid bug collect all object has this script
                string objName = scriptInteract.GetObjectName();
                if(objName != null)
                {
                    infoText.text = objName;//get name object to show
                    infoUI.SetActive(true);//show
                }
                else
                {
                    infoUI.SetActive(false);
                }
            }
            else
            {
                ObjectScriptedCurrent = null;//ko nhat
                infoUI.SetActive(false);//if no script no show
            }
        }
        else
        {
            ObjectScriptedCurrent = null;//ko cho nhat
            infoUI.SetActive(false);// avoid bug from scripted object -> no hitted object(T->NG)  
        }

        // XU LY BAM CHUOT TRAI DE CHAT CAY and COOLDOWN
        if (Input.GetMouseButtonDown(0) && Time.time > nextAttack)
        {
            playerMovement.Attack();
            nextAttack = Time.time + atackRate;//cooldown
            if (ObjectScriptedCurrent != null)
            {
                //type = harvestable
                if (ObjectScriptedCurrent.interactType == InteractType.Harvestable)
                {
                    //is equiped
                    if (EquipManager.Instance != null && EquipManager.Instance.currentEquipped != null)
                    {
                        ObjectScriptedCurrent.TakeHit(EquipManager.Instance.currentEquipped);
                    }
                    else
                    {
                        Debug.Log("Ban can cam cong cu (Axe/Stone) de chat/dap!");
                    }
                }
            }
            
        }

        // XU LY BAM E DE NHAT DO (Collectable)
        if(Input.GetKeyDown(KeyCode.E) && ObjectScriptedCurrent != null)
        {
            if (ObjectScriptedCurrent.interactType == InteractType.InfoOnly || ObjectScriptedCurrent.interactType == InteractType.Harvestable)
            {
                return; // Chi nhat do voi Collectable, con cay thi phai chuot trai
            }

            ItemData itemData = ObjectScriptedCurrent.itemData;
            if(itemData == null)
            {
                Debug.LogWarning("Object has no itemData assigned: " + ObjectScriptedCurrent.ObjectName);
                return;
            }
            if(!InventoryManager.Instance.CheckFullSlot())//if not full
            {
                //save itemData BEFORE destroy to avoid null reference
                Destroy(ObjectScriptedCurrent.gameObject);//xoa object chua script hien tai dang chon
                ObjectScriptedCurrent = null;//clear reference after destroy
                //call func add item
                InventoryManager.Instance.addItem(itemData);
                Debug.Log("Collected new item: " + itemData.itemName);
            }
            else
            {
                Debug.Log("Inventory is full!");
            }
        }
    }
}
