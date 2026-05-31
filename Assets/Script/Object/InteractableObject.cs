using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractType { Collectable, Harvestable, InfoOnly }

public class InteractableObject : MonoBehaviour
{
    public bool isInRange;
    public ItemData itemData;
    public InteractType interactType = InteractType.Collectable;

    [Header("Harvesting Settings (if Harvestable)")]
    public int maxHits = 3;
    public int dropAmount = 2;
    private int currentHits = 0;


    public string GetObjectName()
    {
        if (itemData != null)
            return itemData.itemName;
        return gameObject.name;//fallback neu chua gan itemData hoac la cay (Harvestable/InfoOnly)
    }

    public int GetCurrentHits()
    {
        return currentHits;
    }

    public void TakeHit(int damage)
    {
        if (interactType != InteractType.Harvestable) return;//if click to unharvestable -> none

        if (damage > 0)
        {
            currentHits += damage;
            Debug.Log(GetObjectName() + " bi danh sat thuong " + damage + ". Tong: " + currentHits + "/" + maxHits);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHit3D(transform.position);
            }


            if (currentHits >= maxHits)
            {
                Harvest();
            }
        }
        else
        {
            Debug.Log("Ban can cam Axe hoac Tool phu hop de chat/dap " + GetObjectName());
        }
    }

    private void Harvest()
    {
        if (itemData != null && itemData.prefab3D != null && dropAmount > 0)
        {
            for (int i = 0; i < dropAmount; i++)
            {
                // Spawn random xung quanh
                Vector3 randomOffset = new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
                Vector3 spawnPos = transform.position + randomOffset;
                // detect on terrain high and low
                if (Terrain.activeTerrain != null)
                {
                    float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPos) + Terrain.activeTerrain.transform.position.y;
                    spawnPos.y = terrainHeight + 0.1f; //avoid under ground
                }
                else
                {
                    // second way use ray to ground
                    Vector3 rayStart = spawnPos;
                    rayStart.y += 10f; // ray on top 10m

                    // use layermask to skip trigger or player 
                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f))
                    {
                        spawnPos.y = hit.point.y + 0.1f;
                    }
                    else
                    {
                        spawnPos.y = transform.position.y + 0.5f; //last check if no touch anything
                    }
                }
                Instantiate(itemData.prefab3D, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning(GetObjectName() + " khong co ItemData hoac prefab3D de drop!");
        }
        Debug.Log("Khai thac xong " + GetObjectName());
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isInRange = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
        }
    }
}
