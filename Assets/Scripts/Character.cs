using UnityEngine;

[RequireComponent(typeof(InputManager), typeof(Rigidbody))]
public class Character : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(1f, 100f)][SerializeField] private float maxSpeed = 10f;
    [Range(1f, 100f)][SerializeField] private float maxAcceleration = 10f;
    [Range(1f, 100f)][SerializeField] private float maxAirAcceleration = 1f;

    [Header("Jump Settings")]
    [Range(1f, 10f)][SerializeField] private float jumpHeight = 2f;
    [Range(1, 5)] private readonly int maxAirJumps = 2;

    private Vector3 velocity, desiredVelocity = Vector3.zero;
    private InputManager input;
    private Rigidbody rb;

    private bool isGrounded = false;
    private bool desiredJump = false;
    private int jumpPhase = 0;

    private void Jump()
    {
        if (!isGrounded && jumpPhase >= maxAirJumps) return;

        // Jump speed overcoming gravity formula
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);

        // Canceling an already existing vertical velocity prevents sequential jumps to go higher than intended
        if (velocity.y > 0f) jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
        velocity.y += jumpSpeed;

        // increase internal jump count
        jumpPhase++;
    }

    private void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            // If contact happend on a surface point mostly up, then is grounded
            isGrounded |= normal.y >= 0.9f;
        }
    }

    private void UpdateState()
    {
        velocity = rb.linearVelocity;

        if (isGrounded) jumpPhase = 0;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputManager>();
        input.CreateInputMap();
    }

    private void Update()
    {
        desiredVelocity = new Vector3(input.Movement.x, 0f, input.Movement.y) * maxSpeed;
        desiredJump |= input.Jump.WasPressedThisFrame();
    }

    private void FixedUpdate()
    {
        UpdateState();

        float accelaration = isGrounded ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = accelaration * Time.deltaTime;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

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
