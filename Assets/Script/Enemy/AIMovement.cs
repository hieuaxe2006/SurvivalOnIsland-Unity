using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [SerializeField] private float detectRange = 10f;
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderTimer = 4f;
    private float timer;
    private float norSpeed;

    private Animator anm;

    [Header("Fighting Setting")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1.5f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    void Start()
    {
        anm = GetComponent<Animator>();
        timer = wanderTimer;
        norSpeed = agent.speed;

        // Auto-find player by tag if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                SurvivalStats stats = FindObjectOfType<SurvivalStats>();
                if (stats != null)
                {
                    player = stats.transform;
                }
                else
                {
                    PlayerMovement movement = FindObjectOfType<PlayerMovement>();
                    if (movement != null)
                    {
                        player = movement.transform;
                    }
                }
            }
        }

        if (player == null)
        {
            Debug.LogError("[AIMovement] Player not found! Ensure Player has tag 'Player' or SurvivalStats/PlayerMovement script.", this);
        }
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        float distance = Vector3.Distance(transform.position, player.position);
        //if player in range
        if (distance <= detectRange)
        {
            //buff speed
            agent.speed = norSpeed * 2;

            if (distance <= attackRange && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;//cooldown
                StartCoroutine(AttackSequence());//start attack sequence
            }
            else
            {
                agent.SetDestination(player.position);//follow player

                // Rotate towards player
                Vector3 lookPos = player.position - transform.position;
                lookPos.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), 5 * Time.deltaTime);
            }
        }
        else
        {
            // Wander mode
            agent.speed = norSpeed;
            timer += Time.deltaTime;
            if (timer >= wanderTimer)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
                timer = 0;
            }
        }

        // Update animation
        if (anm != null)
        {
            float speedPercent = agent.velocity.magnitude / agent.speed;
            anm.SetFloat("Speed", speedPercent);
        }
    }

    // Attack sequence: stop, face player, trigger animation, deal damage
    private IEnumerator AttackSequence()
    {
        isAttacking = true;

        // Stop movement
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
        }

        // Face the player
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookPos);
        }

        // Trigger attack animation
        if (anm != null)
        {
            anm.SetTrigger("Attack");
        }

        // Wait for attack hit timing
        yield return new WaitForSeconds(0.5f);

        // Deal damage if player still in range
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            if (SurvivalStats.Instance != null && !SurvivalStats.Instance.IsDead)
            {
                SurvivalStats.Instance.TakeDamage(attackDamage);
                Debug.Log($"[AIMovement] Hit player! Dealt {attackDamage} damage. Player HP: {SurvivalStats.Instance.CurrentHealth}");
            }
        }

        // Wait for attack animation to finish
        yield return new WaitForSeconds(0.5f);

        // Resume movement
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
        }
        isAttacking = false;
    }

    /// <summary>Returns a random position on the NavMesh within the given radius.</summary>
    public static Vector3 RandomNavSphere(Vector3 pos, float radius, int layerMask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += pos;
        NavMeshHit agentHit;
        // Sample nearest valid NavMesh position to avoid obstacles
        NavMesh.SamplePosition(randomDirection, out agentHit, radius, layerMask);
        return agentHit.position;
    }
}
