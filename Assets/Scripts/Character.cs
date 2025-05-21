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
    private Renderer rend;

    // Runtime variables
    private Vector3 velocity, desiredVelocity = Vector3.zero;
    private Vector3 contactNormal = Vector3.zero;
    private bool desiredJump = false;
    private int groundContactCount = 0;
    private int stepsSinceLastGrounded = 0;
    private int jumpPhase = 0;
    private float minGroundDotProduct = 0f;

    private bool OnGround => groundContactCount > 0;

    private void Jump()
    {
        if (!OnGround && jumpPhase >= maxAirJumps) return;

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
                groundContactCount++;
                contactNormal += normal;
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

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    private bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1) return false;
        if (!Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit)) return false;
        if (hit.normal.y < minGroundDotProduct) return false;

        groundContactCount = 1;
        contactNormal = hit.normal;
        float speed = velocity.magnitude;
        float dot = Vector3.Dot(velocity, contactNormal);
        if (dot > 0f) velocity = (velocity - contactNormal * dot).normalized * speed;
        return true;
    }

    private void UpdateState()
    {
        velocity = rb.linearVelocity;
        stepsSinceLastGrounded++;

        if (OnGround || SnapToGround())
        {
            jumpPhase = 0;
            stepsSinceLastGrounded = 0;

            // Only normalize contact if it is an aggregate
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else contactNormal = Vector3.up;
    }

    private void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        input = GetComponent<InputManager>();
        input.CreateInputMap();

        OnValidate();
    }

    private void Update()
    {
        rend.material.SetColor("_Color", Color.white * (groundContactCount * 0.25f));

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

        ClearState();
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
