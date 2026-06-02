using UnityEngine;

public enum ItemType { Material, Tool, Weapon, Food, Placeable, QuestItem }

[CreateAssetMenu(menuName = "Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public int maxStack = 100;
    public GameObject prefab3D;
    public Vector3 equipPos;
    public Vector3 equipRot;
    public int damage;
    public int treeDamage;
    public int healAmount;
}
