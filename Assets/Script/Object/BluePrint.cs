using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//material info
[System.Serializable]
public class MaterialRequirement
{
    public ItemData item;
    public int quantity;
}

//blueprint info
[CreateAssetMenu(menuName = "Blueprint")]//m2 -> Create -> Blueprint
public class BluePrint : ScriptableObject
{
    public string blueprintName;
    public List<MaterialRequirement> materials;
    public ItemData resultItem;
}
