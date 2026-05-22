using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeerAI : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [SerializeField] private float detectRange = 10f;
    [SerializeField] private float fleeDistance = 15f; // Khoang cach chay tron
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderTimer = 4f;

    private float timer;
    private float norSpeed;
    private Animator anm;

    void Start()
    {
        anm = GetComponent<Animator>();
        timer = wanderTimer;
        norSpeed = agent.speed;

        // Neu chua gan player tu inspector thi tu tim
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectRange) 
        {
            // CHAY TRON
            agent.speed = norSpeed * 1.5f; // Huou chay nhanh hon binh thuong

            Vector3 dirToPlayer = transform.position - player.position;
            Vector3 fleePos = transform.position + dirToPlayer.normalized * fleeDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleePos, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                // Neu ko tim duoc diem chay tron, cu chay thang theo huong nguoc lai
                agent.SetDestination(fleePos);
            }
        }
        else
        {
            // WANDER (Di dao)
            agent.speed = norSpeed;
            timer += Time.deltaTime;

            if(timer >= wanderTimer)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
                timer = 0;
            }
        }

        // Animator
        if (anm != null)
        {
            float speedPercent = agent.velocity.magnitude / (norSpeed * 1.5f);
            anm.SetFloat("Speed", speedPercent);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 pos, float radius, int layerMask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += pos;
        NavMeshHit agentHit;
        NavMesh.SamplePosition(randomDirection, out agentHit, radius, layerMask);
        return agentHit.position;
    }
}
