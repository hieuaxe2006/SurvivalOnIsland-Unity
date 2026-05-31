using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager instance;//change to singleton for more accessable

    [SerializeField] private GameObject infoUI;
    private TMP_Text infoText;
    [Header("Cooldown")]
    [SerializeField] private float atackRate = 1f;//cooldown
    private float nextAttack = 0f;

    [Header("Hold E Settings")]
    [SerializeField] private float AirplanePartHoldTime = 15f;
    private float holdETimer = 0f;
    private InteractableObject lastHeldObject;

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

        // Tự động tắt Raycast Target trên InfoPanel và các đối tượng con của nó
        // để tránh việc bảng hiển thị tên chặn các cú click chuột vào hòm đồ/UI phía sau
        if (infoUI != null)
        {
            var graphics = infoUI.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            foreach (var g in graphics)
            {
                g.raycastTarget = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Kiem tra xem co dang trong hoi thoai ko
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive())
        {
            if (infoUI != null) infoUI.SetActive(false);
            return;
        }

        //tao raycast
        Vector3 mousePos = Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);//lay ray tu screen -> mouse position
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

        // xu ly chuot trai de chat cay, danh quai, dat lua trai, eat | Cooldown
        bool isLeftClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (isLeftClick && Time.time > nextAttack && (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()))
        {
            ItemData currentTool = (EquipManager.Instance != null) ? EquipManager.Instance.currentEquipped : null;

            // Eat - no delay hit
            if (currentTool != null && currentTool.itemType == ItemType.Food)
            {
                //if equip force meat
                Animator anim = playerMovement.GetComponentInChildren<Animator>();  
                if (anim != null)
                {
                    anim.SetTrigger("Eat"); //set anm
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy Animator trên Player hoặc các object con của Player.");
                }
                SurvivalStats.Instance.Consume(currentTool);
                InventoryManager.Instance.RemoveItem(currentTool.itemName, 1);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayClick();
                }

                // if last item so unequip
                if (InventoryManager.Instance.GetItemCount(currentTool.itemName) <= 0)
                {
                    EquipManager.Instance.Unequip();
                }
                
                nextAttack = Time.time + atackRate; // cooldown
                return;
            }

            // Campfire | placeable item
            if (currentTool != null && currentTool.itemType == ItemType.Placeable)
            {
                // Raycast to ground (max 100m to ensure aim to ground)
                if (Physics.Raycast(ray, out RaycastHit groundHit, 100f)) 
                {
                    // Kiem tra khoang cach tu nguoi choi toi diem dat (toi da 5m)
                    float distanceToPlayer = Vector3.Distance(playerMovement.transform.position, groundHit.point);
                    if (distanceToPlayer <= 5f)
                    {
                        if (currentTool.prefab3D != null)
                        {
                            Instantiate(currentTool.prefab3D, groundHit.point, Quaternion.identity);
                            InventoryManager.Instance.RemoveItem(currentTool.itemName, 1);


                            //if last item
                            if (InventoryManager.Instance.GetItemCount(currentTool.itemName) <= 0)
                            {
                                EquipManager.Instance.Unequip();
                            }
                            Debug.Log("Da dat thanh cong " + currentTool.itemName);
                        }
                        else
                        {
                            Debug.LogWarning("Chua gan Prefab 3D cho " + currentTool.itemName + " trong ItemData!");
                        }
                    }
                    else
                    {
                        Debug.Log("Vi tri qua xa de dat! (" + System.Math.Round(distanceToPlayer, 1) + "m)");
                    }
                    nextAttack = Time.time + atackRate;//cooldown
                }
                else
                {
                    Debug.Log("Khong the dat o day (Raycast tu Camera khong cham dat).");
                }
                return;
            }

            // Atack
            playerMovement.Attack();
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAttack();
            }
            nextAttack = Time.time + atackRate;//cooldown
            
            // setting damage
            int treeDamage = 0;
            float enemyDamage = 1f; // fist default

            if (currentTool != null)
            {
                enemyDamage = currentTool.damage > 0 ? (float)currentTool.damage : 1f;
                treeDamage = currentTool.treeDamage;

                // Legacy fallback if damage/treeDamage is not set in ItemData asset
                if (currentTool.damage == 0)
                {
                    if (currentTool.itemName == "Stone" || currentTool.itemName == "Wood")
                        enemyDamage = 2f;
                    else if (currentTool.itemName == "Axe")
                        enemyDamage = 3f;
                    else if (currentTool.itemName == "Sword")
                        enemyDamage = 5f;
                }

                if (currentTool.treeDamage == 0)
                {
                    if (currentTool.itemName == "Stone" || currentTool.itemName == "Wood")
                        treeDamage = 1;
                    else if (currentTool.itemName == "Axe")
                        treeDamage = 2;
                }
            }

            // Chat cay (Harvestable)
            if (ObjectScriptedCurrent != null && ObjectScriptedCurrent.interactType == InteractType.Harvestable)
            {
                ObjectScriptedCurrent.TakeHit(treeDamage);

                // Hiển thị HP mục tiêu trên HUD
                if (NotificationUI.Instance != null)
                {
                    float currentHP = ObjectScriptedCurrent.maxHits - ObjectScriptedCurrent.GetCurrentHits();
                    NotificationUI.Instance.ShowTargetHP(ObjectScriptedCurrent.GetObjectName(), currentHP, ObjectScriptedCurrent.maxHits);
                }
            }

            // Attack enemies (using raycast hit and distance check)
            if (hit.collider != null)
            {
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {                             
                    // Kiem tra khoang cach de tranh viec danh quai tu qua xa
                    float distanceToEnemy = Vector3.Distance(playerMovement.transform.position, hit.collider.transform.position);
                    if (distanceToEnemy <= 10f)
                    {
                        enemy.TakeHit(enemyDamage);
                    }
                    else
                    {
                        Debug.Log("Quai vat o qua xa de tan cong! (" + System.Math.Round(distanceToEnemy, 1) + "m)");
                    }
                }
            }
        }

        // Collect item
        bool isEPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool isEHeld = Keyboard.current != null && Keyboard.current.eKey.isPressed;

        if (ObjectScriptedCurrent != null)
        {
            if (ObjectScriptedCurrent.interactType != InteractType.InfoOnly && ObjectScriptedCurrent.interactType != InteractType.Harvestable)
            {
                ItemData itemData = ObjectScriptedCurrent.itemData;
                if (itemData != null && itemData.itemName == "AirplanePart")
                {
                    // Logic Giữ E cho AirplanePart (Động cơ máy bay)
                    if (isEHeld)
                    {
                        if (lastHeldObject != ObjectScriptedCurrent)
                        {
                            holdETimer = 0f;
                            lastHeldObject = ObjectScriptedCurrent;
                        }

                        holdETimer += Time.deltaTime;
                        float progress = holdETimer / AirplanePartHoldTime;

                        if (NotificationUI.Instance != null)
                        {
                            NotificationUI.Instance.UpdateProgressNotification($"Salvaging Engine... {Mathf.RoundToInt(progress * 100f)}%");
                        }

                        // Phát tiếng click nhỏ báo hiệu tiến trình tháo gỡ cơ khí (mỗi 0.5s)
                        if (Mathf.FloorToInt(holdETimer * 2f) != Mathf.FloorToInt((holdETimer - Time.deltaTime) * 2f))
                        {
                            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                        }

                        if (holdETimer >= AirplanePartHoldTime)
                        {
                            CollectItem(ObjectScriptedCurrent);
                            holdETimer = 0f;
                            lastHeldObject = null;
                        }
                    }
                    else
                    {
                        // Thả E giữa chừng -> reset
                        if (holdETimer > 0f)
                        {
                            holdETimer = 0f;
                            lastHeldObject = null;
                            if (NotificationUI.Instance != null)
                            {
                                NotificationUI.Instance.ShowNotification("Salvaging interrupted!");
                            }
                        }
                    }
                }
                else if (isEPressed)
                {
                    // Nhặt tức thời cho các vật phẩm thường khác
                    CollectItem(ObjectScriptedCurrent);
                }
            }
        }
        else
        {
            // Nhìn đi chỗ khác -> reset tiến trình
            if (holdETimer > 0f)
            {
                holdETimer = 0f;
                lastHeldObject = null;
                if (NotificationUI.Instance != null)
                {
                    NotificationUI.Instance.ShowNotification("Salvaging interrupted!");
                }
            }
        }
    }

    private void CollectItem(InteractableObject obj)
    {
        if (obj == null) return;
        ItemData itemData = obj.itemData;
        if (itemData == null)
        {
            Debug.LogWarning("Object has no itemData assigned: " + obj.GetObjectName());
            return;
        }

        if (!InventoryManager.Instance.CheckFullSlot())
        {
            Destroy(obj.gameObject);
            if (obj == ObjectScriptedCurrent)
            {
                ObjectScriptedCurrent = null;
            }

            InventoryManager.Instance.addItem(itemData);

            if (NotificationUI.Instance != null)
            {
                NotificationUI.Instance.ShowNotification("Collected: " + itemData.itemName + " x1");
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCollect();
            }

            Debug.Log("Collected new item: " + itemData.itemName);
        }
        else
        {
            if (NotificationUI.Instance != null)
            {
                NotificationUI.Instance.ShowNotification("Inventory is full!");
            }
            Debug.Log("Inventory is full!");
        }
    }
}
