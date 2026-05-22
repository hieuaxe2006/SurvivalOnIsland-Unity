using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//set type item
public enum ItemType { Material, Tool, Weapon, Food, Placeable, QuestItem }

[CreateAssetMenu(menuName = "Item")]//m2->create->data
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public int maxStack = 100;    // 100 for materials/food, 1 for tools/weapons
    public GameObject prefab3D;   // hold 3d item
    public Vector3 equipPos;      // position when equipped
    public Vector3 equipRot;      // rotation when equipped
    public int damage;            // dame weapon type
    public int healAmount;        // For food (hunger/thirst)
}

