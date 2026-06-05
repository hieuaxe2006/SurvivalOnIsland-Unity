using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager instance; // Singleton instance

    [SerializeField] private GameObject infoUI; // Name display panel GameObject
    private TMP_Text infoText;

    [Header("Cooldown Settings")]
    [SerializeField] private float atackRate = 1f; // Weapon/Attack cooldown rate
    private float nextAttack = 0f;

    [Header("Hold E Settings")]
    [SerializeField] private float AirplanePartHoldTime = 15f; // Time in seconds to hold E for AirplanePart
    private float holdETimer = 0f;
    private InteractableObject lastHeldObject;
    private float lookAwayTimer = 0f; // Look-away grace period timer
    private float lookAwayGracePeriod = 0.5f; // Grace period buffer to prevent instant resets due to camera sway

    private InteractableObject ObjectScriptedCurrent; // Currently highlighted interactable object
    private PlayerMovement playerMovement;

    private void Awake()
    {
        // Singleton pattern setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        infoText = infoUI.GetComponent<TMP_Text>();
        playerMovement = FindObjectOfType<PlayerMovement>();

        // Disable raycast target on all elements of the Info Panel to prevent blocking clicks
        if (infoUI != null)
        {
            var graphics = infoUI.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            foreach (var g in graphics)
            {
                g.raycastTarget = false;
            }
        }
    }

    private void Update()
    {
        // Don't interact or raycast if dialogue is currently active
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive())
        {
            if (infoUI != null) infoUI.SetActive(false);
            return;
        }

        // Raycast from camera center to aim at interactable objects
        Vector3 mousePos = Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;
            InteractableObject scriptInteract = selectionTransform.GetComponent<InteractableObject>();

            // If object is interactable and player is within trigger range
            if (scriptInteract && scriptInteract.isInRange)
            {
                ObjectScriptedCurrent = scriptInteract;
                string objName = scriptInteract.GetObjectName();
                if (objName != null)
                {
                    infoText.text = objName;
                    infoUI.SetActive(true); // Display name overlay
                }
                else
                {
                    infoUI.SetActive(false);//hide if no name provided
                }
            }
            else
            {
                ObjectScriptedCurrent = null;
                infoUI.SetActive(false);//hide if not interactable or out of range
            }
        }
        else
        {
            ObjectScriptedCurrent = null;
            infoUI.SetActive(false);//hide if raycast hits nothing
        }

        // Left Click: Handles attacking, harvesting, and consuming
        bool isLeftClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        //if clicked and cooldown has passed
        if (isLeftClick && Time.time > nextAttack && (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()))
        {
            //check item in hand
            ItemData currentTool = (EquipManager.Instance != null) ? EquipManager.Instance.currentEquipped : null;

            // 1. Consume Food/Placeables
            if (currentTool != null && currentTool.itemType == ItemType.Food)
            {
                //anm
                Animator anim = playerMovement.GetComponentInChildren<Animator>();  
                if (anim != null)
                {
                    anim.SetTrigger("Eat");
                }
                // heal hunger/thirst
                SurvivalStats.Instance.Consume(currentTool);
                // -1 from inventory
                InventoryManager.Instance.RemoveItem(currentTool.itemName, 1);
                //sfx
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                //unequip if no more left
                if (InventoryManager.Instance.GetItemCount(currentTool.itemName) <= 0)
                {
                    EquipManager.Instance.Unequip(); 
                }
                //cooldown
                nextAttack = Time.time + atackRate;
                return;
            }

            // 2. Place Placeable Prefabs (e.g. Campfire)
            if (currentTool != null && currentTool.itemType == ItemType.Placeable)
            {//ray to ground to place object
                if (Physics.Raycast(ray, out RaycastHit groundHit, 100f)) 
                {
                    //check distance
                    float distanceToPlayer = Vector3.Distance(playerMovement.transform.position, groundHit.point);
                    if (distanceToPlayer <= 5f) // Max placing distance is 5m
                    {
                        if (currentTool.prefab3D != null)
                        {
                            //create
                            Instantiate(currentTool.prefab3D, groundHit.point, Quaternion.identity);
                            //-1 from inventory
                            InventoryManager.Instance.RemoveItem(currentTool.itemName, 1);
                            //unequip if no more left
                            if (InventoryManager.Instance.GetItemCount(currentTool.itemName) <= 0)
                            {
                                EquipManager.Instance.Unequip();
                            }
                        }
                    }
                    //cooldown
                    nextAttack = Time.time + atackRate;
                }
                return;
            }

            // 3. Regular Attack Animation and Sound
            Animator anm = playerMovement.GetComponentInChildren<Animator>();
            if (anm != null)
            {
                anm.SetTrigger("Hit");
            }
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAttack();
            //cooldown
            nextAttack = Time.time + atackRate;
            
            // Calculate tool/weapon damage values
            float treeDamage = 0;
            float enemyDamage = 1f;

            if (currentTool != null)
            {
                // get damage value, default = 1
                enemyDamage = currentTool.damage > 0 ? (float)currentTool.damage : 1f;
                treeDamage = currentTool.treeDamage;

                // Fallbacks for default tools
                if (currentTool.damage == 0)
                {
                    if (currentTool.itemName == "Stone" || currentTool.itemName == "Wood") enemyDamage = 2f;
                    else if (currentTool.itemName == "Axe") enemyDamage = 3f;
                    else if (currentTool.itemName == "Sword") enemyDamage = 4f;
                }

                if (currentTool.treeDamage == 0)
                {
                    if (currentTool.itemName == "Stone" || currentTool.itemName == "Wood") treeDamage = 0.5f;
                    else if (currentTool.itemName == "Axe") treeDamage = 1.5f;
                }
            }

            // 4. Attack Harvestable Objects (Trees, Rocks)
            if (ObjectScriptedCurrent != null && ObjectScriptedCurrent.interactType == InteractType.Harvestable)
            {
                ObjectScriptedCurrent.TakeHit(treeDamage);

                if (NotificationUI.Instance != null)
                {
                    float currentHP = ObjectScriptedCurrent.maxHits - ObjectScriptedCurrent.GetCurrentHits();
                    NotificationUI.Instance.ShowTargetHP(ObjectScriptedCurrent.GetObjectName(), currentHP, ObjectScriptedCurrent.maxHits);
                }
            }

            // 5. Attack Enemies within 10m range
            if (hit.collider != null)
            {
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {                             
                    float distanceToEnemy = Vector3.Distance(playerMovement.transform.position, hit.collider.transform.position);
                    if (distanceToEnemy <= 10f)
                    {
                        enemy.TakeHit(enemyDamage);
                    }
                }
            }
        }

        // E Key Press / Hold interactions
        bool isEPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool isEHeld = Keyboard.current != null && Keyboard.current.eKey.isPressed;

        if (ObjectScriptedCurrent != null)
        {
            if (ObjectScriptedCurrent.interactType != InteractType.InfoOnly && ObjectScriptedCurrent.interactType != InteractType.Harvestable)
            {
                ItemData itemData = ObjectScriptedCurrent.itemData;
                
                // AirplanePart requires holding E
                if (itemData != null && itemData.itemName == "AirplanePart")
                {
                    if (isEHeld)
                    {
                        if (lastHeldObject != ObjectScriptedCurrent)
                        {
                            holdETimer = 0f;
                            lastHeldObject = ObjectScriptedCurrent;
                        }

                        lookAwayTimer = 0f; // Reset time if out sight
                        holdETimer += Time.deltaTime;
                        float progress = holdETimer / AirplanePartHoldTime;

                        if (NotificationUI.Instance != null)
                        {
                            NotificationUI.Instance.UpdateProgressNotification($"Salvaging Engine... {Mathf.RoundToInt(progress * 100f)}%");
                        }

                        // Play ticking click sound every 0.5s of progress
                        if (Mathf.FloorToInt(holdETimer * 2f) != Mathf.FloorToInt((holdETimer - Time.deltaTime) * 2f))
                        {
                            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
                        }

                        // Completed holds
                        if (holdETimer >= AirplanePartHoldTime)
                        {
                            CollectItem(ObjectScriptedCurrent);
                            holdETimer = 0f;
                            lastHeldObject = null;
                            lookAwayTimer = 0f;
                        }
                    }
                    else
                    {
                        // Release E resets immediately
                        if (holdETimer > 0f)
                        {
                            ResetHoldProgress("Salvaging interrupted!");
                        }
                    }
                }
                else if (isEPressed)
                {
                    // Regular items are picked up instantly on press
                    CollectItem(ObjectScriptedCurrent);
                }
            }
        }
        else
        {
            // Grace period look-away logic when still holding E
            if (lastHeldObject != null && isEHeld)
            {
                lookAwayTimer += Time.deltaTime;
                if (lookAwayTimer >= lookAwayGracePeriod)
                {
                    ResetHoldProgress("Salvaging interrupted!");
                }
            }
            else
            {
                if (holdETimer > 0f)
                {
                    ResetHoldProgress("Salvaging interrupted!");
                }
            }
        }
    }
    //handle holding reset
    private void ResetHoldProgress(string message)
    {
        holdETimer = 0f;
        lastHeldObject = null;
        lookAwayTimer = 0f;
        if (NotificationUI.Instance != null)
        {
            NotificationUI.Instance.ShowNotification(message);
        }
    }

    // Handle collect
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
            // Save item to player pref
            //save itemdata + pos
            string itemID = $"collected_{obj.gameObject.name}_{obj.transform.position.x:F2}_{obj.transform.position.y:F2}_{obj.transform.position.z:F2}";
            PlayerPrefs.SetInt(itemID, 1);
            PlayerPrefs.Save();

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
        }
        else
        {
            if (NotificationUI.Instance != null)
            {
                NotificationUI.Instance.ShowNotification("Inventory is full!");
            }
        }
    }
}
