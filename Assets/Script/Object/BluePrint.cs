using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialRequirement
{
    public ItemData item;
    public int quantity;
}

[CreateAssetMenu(menuName = "Blueprint")]
public class BluePrint : ScriptableObject
{
    public string blueprintName;
    public List<MaterialRequirement> materials;
    public ItemData resultItem;
}
