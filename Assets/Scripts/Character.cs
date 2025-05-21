using UnityEngine;

[RequireComponent(typeof(InputManager), typeof(Rigidbody))]
public class Character : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0f, 100f)][SerializeField] private float maxSpeed = 10f;
    [Range(0f, 100f)][SerializeField] private float maxAcceleration = 10f;
    [Range(0f, 100f)][SerializeField] private float maxAirAcceleration = 1f;
    [Range(0f, 90f)][SerializeField] private float maxGroundAngle = 25f;

    [Header("Jump Settings")]
    [Range(1f, 10f)][SerializeField] private float jumpHeight = 2f;
    [Range(1, 5)] private readonly int maxAirJumps = 2;

    // Assigned on awake (don't change)
    private InputManager input;
    private Rigidbody rb;

    // Runtime variables
    private Vector3 velocity, desiredVelocity = Vector3.zero;
    private Vector3 contactNormal = Vector3.zero;
    private bool isGrounded = false;
    private bool desiredJump = false;
    private int jumpPhase = 0;
    private float minGroundDotProduct = 0f;

    private void Jump()
    {
        if (!isGrounded && jumpPhase >= maxAirJumps) return;

        // Jump speed overcoming gravity formula
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);

        // Canceling an already existing vertical velocity prevents sequential jumps to go higher than intended
        float alignedSpeed = Vector3.Dot(velocity, contactNormal);
        if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        velocity += contactNormal * jumpSpeed;

        // increase internal jump count
        jumpPhase++;
    }

    private void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                isGrounded = true;
                contactNormal = normal;
            }
        }
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    private void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = isGrounded ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    private void UpdateState()
    {
        velocity = rb.linearVelocity;

        if (isGrounded) jumpPhase = 0;
        else contactNormal = Vector3.up;
    }

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputManager>();
        input.CreateInputMap();

        OnValidate();
    }

    private void Update()
    {
        desiredVelocity = new Vector3(input.Movement.x, 0f, input.Movement.y) * maxSpeed;
        desiredJump |= input.Jump.WasPressedThisFrame();
    }

    private void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        rb.linearVelocity = velocity;
        isGrounded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }
}
