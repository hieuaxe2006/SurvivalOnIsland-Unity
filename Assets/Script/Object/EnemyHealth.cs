using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 10f;
    public float currentHealth;

    [Header("UI Settings")]
    public Image healthBarFill;
    public GameObject healthBarCanvas;

    [Header("Drop Settings")]
    public ItemData dropItem;
    public int dropAmount = 2;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        // Rotate health bar to face camera
        if (healthBarCanvas != null && Camera.main != null)
        {
            healthBarCanvas.transform.rotation = Quaternion.LookRotation(healthBarCanvas.transform.position - Camera.main.transform.position);
        }
    }

    /// <summary>Applies damage to this enemy.</summary>
    public void TakeHit(float damage)
    {
        if (isDead || damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHit3D(transform.position);
        }

        Debug.Log(gameObject.name + " took " + damage + " damage. HP remaining: " + currentHealth);

        // Show target HP on HUD
        if (NotificationUI.Instance != null)
        {
            NotificationUI.Instance.ShowTargetHP(gameObject.name, currentHealth, maxHealth);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        isDead = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeath3D(transform.position);
        }

        // Hide health bar UI
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }

        // Drop items
        if (dropItem != null && dropItem.prefab3D != null && dropAmount > 0)
        {
            for (int i = 0; i < dropAmount; i++)
            {
                // Spawn at random offset around death position
                Vector3 randomOffset = new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
                Vector3 spawnPos = transform.position + randomOffset;

                if (Terrain.activeTerrain != null)
                {
                    float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPos) + Terrain.activeTerrain.transform.position.y;
                    spawnPos.y = terrainHeight + 0.1f;
                }
                else
                {
                    spawnPos.y = transform.position.y + 0.5f;
                }

                Instantiate(dropItem.prefab3D, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning(gameObject.name + " has no drop item data!");
        }

        Debug.Log("Defeated " + gameObject.name);
        Destroy(gameObject);
    }
}
