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

    // Start is called before the first frame update
    void Start()
    {
        //get component
        anm = GetComponent<Animator>();
        //set timer 
        timer = wanderTimer;
        //set Speed
        norSpeed = agent.speed;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);//lay khoang cach

        if (distance <= detectRange) //neu in range
        {
            agent.speed = norSpeed*2;//x2 speed
            agent.SetDestination(player.position);//dat aim player
            //aim
            Vector3 lookPos = player.position - transform.position;//lay huong tu AI->Player
            lookPos.y = 0;//avoid aim to body player
            //rotate, slerp(from, to, speedRotate)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), 5*Time.deltaTime);
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
        float speedAI = agent.velocity.magnitude;//speed realtime
        float speedPercent = agent.velocity.magnitude / agent.speed;
        anm.SetFloat("Speed", speedPercent);
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
