using UnityEngine;
public class SurvivalStats : MonoBehaviour
{
    public static SurvivalStats Instance { get; private set; }

    [Header("health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float currentHunger = 100f;
    [Tooltip("decrease 1% each 3 secs")]
    [SerializeField] private float hungerDecreaseRate = 0.333f; 

    [Header("thirst")]
    [SerializeField] private float maxThirst = 100f;
    [SerializeField] private float currentThirst = 100f;
    [Tooltip("decrease 1% each 3 secs")]
    [SerializeField] private float thirstDecreaseRate = 0.333f;

    [Header("damage get when out of hungry or thirst")]
    [Tooltip("nerf hp each sec")]
    [SerializeField] private float starveDamageRate = 5f;

    public bool IsDead { get; private set; }
    private float nextHurtSoundTime = 0f;

    // get method
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
        //avoid bug
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

        //nerf follow time
        currentHunger -= hungerDecreaseRate * Time.deltaTime;
        currentThirst -= thirstDecreaseRate * Time.deltaTime;

        // limit current hunger/thirst in (0,max)
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);

        // Starve damage
        if (currentHunger <= 0f || currentThirst <= 0f)
        {
            TakeDamage(starveDamageRate * Time.deltaTime);
        }
        else
        {
            //regen 1%HP each 1sec
            Heal(maxHealth * 0.01f * Time.deltaTime);
        }
    }

    public void Consume(ItemData consumable)
    {
        if (consumable == null || IsDead) return;

        //check food or drink
        if (consumable.itemName == "Coconut")
        {
            currentThirst += 15;
            currentThirst = Mathf.Clamp(currentThirst, 0f, maxThirst);
            Debug.Log($"Uống nước dừa! Khát: {currentThirst}");
        }
        else
        {
            currentHunger += 15;
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
            Debug.Log($"Ăn thức ăn! Đói: {currentHunger}");
        }
    }

    //func attack damage on player
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        // Khong nhan damage khi da hoan thanh game (tranh bug quai can chet sau khi da win)
        if (QuestManager.Instance != null && QuestManager.Instance.currentState == QuestState.Completed)
        {
            return;
        }

        // Phát âm thanh khi mất máu (giới hạn tần suất 1s/lần và chỉ với damage thực tế >= 1)
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
    //func regen
    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }
    //func die
    private void Die()
    {
        IsDead = true;
        Debug.Log("[SurvivalStats] Người chơi đã tử vong!");

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
            Debug.LogWarning("[SurvivalStats] Không tìm thấy Animator trên Player để chạy animation Die!");
        }

        if (DeathUI.Instance != null)
        {
            DeathUI.Instance.TriggerDeathScreen();
        }
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        IsDead = false;
        Debug.Log("[SurvivalStats] Người chơi đã hồi sinh!");
    }

    public void RestoreStats(float health, float hunger, float thirst)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        currentHunger = Mathf.Clamp(hunger, 0f, maxHunger);
        currentThirst = Mathf.Clamp(thirst, 0f, maxThirst);
        IsDead = false;
        Debug.Log($"[SurvivalStats] Restored Stats - HP: {currentHealth}, Hunger: {currentHunger}, Thirst: {currentThirst}");
    }
}
