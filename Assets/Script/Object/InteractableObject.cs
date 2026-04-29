using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private string ObjectName;

    public string getObjectName()
    {
        return ObjectName;
    }    
}
