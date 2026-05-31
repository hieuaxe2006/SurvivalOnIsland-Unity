using UnityEngine;

public class PlaneTakeOff : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 10f;      // tốc độ ban đầu
    public float acceleration = 5f;       // gia tốc
    public float climbSpeed = 2f;         // tốc độ bay lên ban đầu
    public float climbAcceleration = 1f;  // gia tốc bay lên

    private float currentForwardSpeed;
    private float currentClimbSpeed;

    void Start()
    {
        currentForwardSpeed = forwardSpeed;
        currentClimbSpeed = climbSpeed;
    }

    void Update()
    {
        // Tăng tốc dần
        currentForwardSpeed += acceleration * Time.deltaTime;
        currentClimbSpeed += climbAcceleration * Time.deltaTime;

        // Máy bay của bạn hướng theo trục X local
        transform.position += transform.right * currentForwardSpeed * Time.deltaTime;

        // Bay lên
        transform.position += Vector3.up * currentClimbSpeed * Time.deltaTime;
    }
}