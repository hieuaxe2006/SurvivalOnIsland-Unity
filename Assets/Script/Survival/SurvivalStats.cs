using UnityEngine;

public class SurvivalStats : MonoBehaviour
{
    public static SurvivalStats Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float currentHunger = 100f;
    [Tooltip("Decrease 1% each 3 secs")]
    [SerializeField] private float hungerDecreaseRate = 0.333f;

    [Header("Thirst")]
    [SerializeField] private float maxThirst = 100f;
    [SerializeField] private float currentThirst = 100f;
    [Tooltip("Decrease 1% each 3 secs")]
    [SerializeField] private float thirstDecreaseRate = 0.333f;

    [Header("Starvation Damage")]
    [Tooltip("HP damage per second when starving or dehydrated")]
    [SerializeField] private float starveDamageRate = 5f;

    public bool IsDead { get; private set; }
    private float nextHurtSoundTime = 0f;

    public float HealthPercent => currentHealth / maxHealth;
    public float HungerPercent => currentHunger / maxHunger;
    public float ThirstPercent => currentThirst / maxThirst;

    public float CurrentHealth => currentHealth;
    public float CurrentHunger => currentHunger;
    public float CurrentThirst => currentThirst;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Clamp to safe defaults
        if (maxHealth <= 0) maxHealth = 100f;
        if (maxHunger <= 0) maxHunger = 100f;
        if (maxThirst <= 0) maxThirst = 100f;

        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        IsDead = false;
    }

    private void Update()
    {
        if (IsDead) return;

        // Decrease over time
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        currentThirst -= thirstDecreaseRate * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);

        // Apply starvation damage or regenerate HP
        if (currentHunger <= 0f || currentThirst <= 0f)
        {
            TakeDamage(starveDamageRate * Time.deltaTime);
        }
        else
        {
            // Regen 1% HP per second
            Heal(maxHealth * 0.01f * Time.deltaTime);
        }
    }

    /// <summary>Consumes an item, restoring hunger or thirst.</summary>
    public void Consume(ItemData consumable)
    {
        if (consumable == null || IsDead) return;

        if (consumable.itemName == "Coconut")
        {
            currentThirst += 15;
            currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
            Debug.Log($"Drank coconut water! Thirst: {currentThirst}");
        }
        else
        {
            currentHunger += 15;
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
            Debug.Log($"Ate food! Hunger: {currentHunger}");
        }
    }

    /// <summary>Applies damage to the player.</summary>
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        // Skip damage if game is already completed
        if (QuestManager.Instance != null && QuestManager.Instance.currentState == QuestState.Completed)
        {
            return;
        }

        // Play hurt sound (throttled to once per second, only for significant damage)
        if (amount >= 1f && Time.time >= nextHurtSoundTime)
        {
            nextHurtSoundTime = Time.time + 1.0f;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHurt();
            }
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>Heals the player by the given amount.</summary>
    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log("[SurvivalStats] Player has died!");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxDeath);
        }

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        else
        {
            Debug.LogWarning("[SurvivalStats] No Animator found on Player for Die animation!");
        }

        if (DeathUI.Instance != null)
        {
            DeathUI.Instance.TriggerDeathScreen();
        }
    }

    /// <summary>Respawns the player with full stats.</summary>
    public void Respawn()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        IsDead = false;
        Debug.Log("[SurvivalStats] Player has respawned!");
    }

    /// <summary>Restores stats from saved data.</summary>
    public void RestoreStats(float health, float hunger, float thirst)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        currentHunger = Mathf.Clamp(hunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(thirst, 0f, maxThirst);
        IsDead = false;
        Debug.Log($"[SurvivalStats] Restored Stats - HP: {currentHealth}, Hunger: {currentHunger}, Thirst: {currentThirst}");
    }
}
