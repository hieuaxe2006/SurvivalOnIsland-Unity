using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;//tao agent cho AI move
    [SerializeField] private Transform player;

    [SerializeField] private float detectRange = 10f;//range phat hien player
    [SerializeField] private float wanderRadius = 5f;//R range move free
    [SerializeField] private float wanderTimer = 4f;//move free 4s
    private float timer;
    private float norSpeed;

    private Animator anm;

    [Header("Fighting Setting")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    // Start is called before the first frame update
    void Start()
    {
        //get component
        anm = GetComponent<Animator>();
        //set timer 
        timer = wanderTimer;
        //set Speed
        norSpeed = agent.speed;

        // Tự động tìm Player bằng tag nếu chưa gán
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                //
                SurvivalStats stats = FindObjectOfType<SurvivalStats>();
                if (stats != null)
                {
                    player = stats.transform;
                }
                else
                {
                    // 
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
            Debug.LogError("[AIMovement] Không tìm thấy Player trong Scene! Hãy đảm bảo Player có tag 'Player' hoặc có gắn script SurvivalStats/PlayerMovement.", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || isAttacking) return;

        float distance = Vector3.Distance(transform.position, player.position);//lay khoang cach

        if (distance <= detectRange) //neu in range
        {
            agent.speed = norSpeed*2;//x2 speed
            
            // Attack player
            if (distance <= attackRange && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                StartCoroutine(AttackSequence());
            }
            else
            {
                agent.SetDestination(player.position);//dat aim player
                
                //aim rotate
                Vector3 lookPos = player.position - transform.position;//lay huong tu AI->Player
                lookPos.y = 0;//avoid aim to body player
                //rotate, slerp(from, to, speedRotate)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), 5*Time.deltaTime);
            }
        }
        else//neu out range(normal)
        {
            //AI will move wander
            agent.speed = norSpeed;
            //tang timer
            timer += Time.deltaTime;
            if(timer >= wanderTimer)//neu timer>chu ky
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);//lay vi tri tu ham tao random
                agent.SetDestination(newPos);//dat aim newposx`
                //reset timer
                timer = 0;
            }
        }
        
        //set anm
        if (anm != null)
        {
            float speedPercent = agent.velocity.magnitude / agent.speed;
            anm.SetFloat("Speed", speedPercent);
        }
    }

    //func attack stand and trigger animation
    private IEnumerator AttackSequence()
    {
        isAttacking = true;

        // Dừng di chuyển
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
        }

        // Quay mặt về hướng Player
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookPos);
        }

        // Kích hoạt hoạt ảnh tấn công
        if (anm != null)
        {
            anm.SetTrigger("Attack");
        }

        // Đợi 0.5 giây khớp với thời điểm vung tay cào trúng của hoạt ảnh
        yield return new WaitForSeconds(0.5f);

        // Gây sát thương nếu người chơi vẫn ở trong tầm
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            if (SurvivalStats.Instance != null && !SurvivalStats.Instance.IsDead)
            {
                SurvivalStats.Instance.TakeDamage(attackDamage);
                Debug.Log($"[AIMovement] Quái cào trúng người chơi! Gây {attackDamage} sát thương. Máu người chơi còn: {SurvivalStats.Instance.CurrentHealth}");
            }
        }

        // Chờ hoàn thành nốt hoạt ảnh tấn công trước khi di chuyển tiếp
        yield return new WaitForSeconds(0.5f);

        // Tiếp tục di chuyển
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
        }
        isAttacking = false;
    }

    //ham tao random
    public static Vector3 RandomNavSphere(Vector3 pos, float radius, int layerMask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;//random huong trong pham vi hinh cau cua AI * R
        randomDirection += pos;//tao vecto random tu AI pos voi R
        NavMeshHit agentHit;
        //avoid obstacle(chuong ngai vat)
        //lay vi tri khac neu ko co navmesh(tranh obstacle) thi move to closest pos in R
        NavMesh.SamplePosition(randomDirection, out agentHit, radius, layerMask);
        return agentHit.position;//new pos
    }
}
