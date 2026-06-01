using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractType { Collectable, Harvestable, InfoOnly }

public class InteractableObject : MonoBehaviour
{
    public bool isInRange; // True when player is inside the interaction trigger zone
    public ItemData itemData; // The item data ScriptableObject associated with this object
    public InteractType interactType = InteractType.Collectable; // Interaction mode (Collectable, Harvestable, InfoOnly)

    [Header("Harvesting Settings (if Harvestable)")]
    public int maxHits = 3; // Number of hits required to harvest this object
    public int dropAmount = 2; // Quantity of items to drop upon harvesting completion
    private int currentHits = 0;

    // Get the clean display name of the object
    public string GetObjectName()
    {
        if (itemData != null)
            return itemData.itemName;
        return gameObject.name; // Fallback to GameObject name if no ItemData assigned
    }

    public int GetCurrentHits()
    {
        return currentHits;
    }

    // Apply hit damage to harvestable objects (e.g. chopping trees or mining rocks)
    public void TakeHit(int damage)
    {
        if (interactType != InteractType.Harvestable) return;

        if (damage > 0)
        {
            currentHits += damage;
            Debug.Log($"{GetObjectName()} hit for {damage} damage. Progress: {currentHits}/{maxHits}");

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
            Debug.Log("You need an Axe or an appropriate tool to harvest " + GetObjectName());
        }
    }

    // Instantiates dropped items randomly on the ground and destroys this object
    private void Harvest()
    {
        if (itemData != null && itemData.prefab3D != null && dropAmount > 0)
        {
            for (int i = 0; i < dropAmount; i++)
            {
                // Calculate random offset to scatter items
                Vector3 randomOffset = new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
                Vector3 spawnPos = transform.position + randomOffset;

                // Adjust Y height to match the Terrain mesh
                if (Terrain.activeTerrain != null)
                {
                    float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPos) + Terrain.activeTerrain.transform.position.y;
                    spawnPos.y = terrainHeight + 0.1f;
                }
                else
                {
                    // Fallback using ground raycast if no Terrain found
                    Vector3 rayStart = spawnPos;
                    rayStart.y += 10f;

                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f))
                    {
                        spawnPos.y = hit.point.y + 0.1f;
                    }
                    else
                    {
                        spawnPos.y = transform.position.y + 0.5f;
                    }
                }
                Instantiate(itemData.prefab3D, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning(GetObjectName() + " is missing ItemData or prefab3D to drop items!");
        }

        Debug.Log("Harvesting completed for " + GetObjectName());

        // Save harvested position state so it does not spawn again when loaded
        string itemID = $"collected_{gameObject.name}_{transform.position.x:F2}_{transform.position.y:F2}_{transform.position.z:F2}";
        PlayerPrefs.SetInt(itemID, 1);
        PlayerPrefs.Save();

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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
