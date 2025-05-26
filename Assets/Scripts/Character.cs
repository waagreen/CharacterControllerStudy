using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Character : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0f, 100f)][SerializeField] private float maxSpeed = 10f;
    [Range(0f, 100f)][SerializeField] private float maxSnapSpeed = 9f;
    [Range(0f, 100f)][SerializeField] private float maxAcceleration = 10f;
    [Range(0f, 100f)][SerializeField] private float maxAirAcceleration = 1f;
    [Range(0f, 90f)][SerializeField] private float maxGroundAngle = 25f;
    [Range(0f, 90f)][SerializeField] private float maxStairAngle = 46f;
    [SerializeField] private Transform playerInputSpace;

    [Header("Jump Settings")]
    [Range(1f, 10f)][SerializeField] private float jumpHeight = 2f;
    [Range(0, 5)][SerializeField] private int maxAirJumps = 2;

    [Header("Raycast Settings")]
    [Min(0f)][SerializeField] private float probeDistance = 1f;
    [SerializeField] private LayerMask probeMask = -1;
    [SerializeField] private LayerMask stairMask = -1;

    // Assigned on awake (don't change)
    private InputManager input;
    private Rigidbody rb;
    private Renderer rend;

    // Runtime variables
    private Vector3 velocity, desiredVelocity = Vector3.zero;
    private Vector3 contactNormal, steepNormal = Vector3.zero;
    private Vector3 upAxis, rightAxis, forwardAxis;
    private bool desiredJump = false;
    private int jumpPhase = 0;
    private int groundContactCount, steepContactCount = 0;
    private int stepsSinceLastGrounded, stepsSinceLastJumped = 0;
    private float minGroundDotProduct, minStairDotProduct = 0f;

    private bool OnGround => groundContactCount > 0;
    private bool OnSteep => steepContactCount > 0;

    float GetMinDot(int layer)
    {
        bool useGroundDot = (stairMask & (1 << layer)) == 0;
        return useGroundDot ? minGroundDotProduct : minStairDotProduct;
    }

    private Vector3 GetJumpDirection()
    {
        if (OnGround)
        {
            //Just standing on a normal surface
            return contactNormal; 
        }
        else if (OnSteep)
        {
            jumpPhase = 0;
            //Steep surfaces comes before air jumps because they need to be treated as ground
            return steepNormal;
        }
        else if ((maxAirJumps > 0) && (jumpPhase <= maxAirJumps))
        {
            if (jumpPhase == 0) jumpPhase = 1;
            //In this case contactNormal was already set to the up axis
            return contactNormal;
        }
        else return Vector3.zero;
    }

    private bool Jump(Vector3 gravity)
    {
        Vector3 jumpDirection = GetJumpDirection();
        if (jumpDirection == Vector3.zero) return false;

        stepsSinceLastJumped = 0;
        jumpPhase++;

        // Jump speed overcoming gravity formula
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);

        // Adding up axis vector to the direction so jumps on steep surfaces provide more vertical velocity
        jumpDirection = (jumpDirection + upAxis).normalized;

        // Canceling an already existing vertical velocity prevents sequential jumps to go higher than intended
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        velocity += jumpDirection * jumpSpeed;
        return true;
    }

    private void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= GetMinDot(collision.gameObject.layer))
            {
                groundContactCount++;
                contactNormal += normal;
            }
            else if (upDot > -0.01f)
            {
                steepContactCount++;
                steepNormal += normal;
            }
        }
    }

    private Vector3 ProjectOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    private void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectOnPlane(forwardAxis, contactNormal);

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
        // Only snaps if just leaved the ground without jumping or after two physics steps after jumping
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJumped <= 2) return false;

        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed) return false;

        // Try to get the ground plane directly below 
        if (!Physics.Raycast(rb.position, -upAxis, out RaycastHit hit, probeDistance, probeMask)) return false;

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer)) return false;

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, contactNormal);
        if (dot > 0f) velocity = (velocity - contactNormal * dot).normalized * speed;
        return true;
    }

    private bool CheckSteepContacts()
    {
        if (steepContactCount < 1) return false;

        float upDot = Vector3.Dot(upAxis, contactNormal);
        if (upDot < minGroundDotProduct) return false;

        // One ground contact represented by the sum of steep normals
        groundContactCount = 1;
        steepNormal.Normalize();
        contactNormal = steepNormal;
        return true;        
    }

    private void UpdateState()
    {
        velocity = rb.linearVelocity;

        stepsSinceLastGrounded++;
        stepsSinceLastJumped++;

        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;

            // Only reset jump phase at least one physics step after the jump was performed
            if (stepsSinceLastJumped > 1) jumpPhase = 0;

            // Only normalize contact if it is an aggregate
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else contactNormal = upAxis;
    }

    private void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    private void SetDesiredVelocity()
    {
        // Project axis on gravity plane 
        if (playerInputSpace)
        {
            rightAxis = ProjectOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectOnPlane(Vector3.forward, upAxis);
        }

        desiredVelocity = new Vector3(input.Movement.x, 0f, input.Movement.y) * maxSpeed;
    }

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairDotProduct = Mathf.Cos(maxStairAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        rend = GetComponent<Renderer>();

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        input = FindFirstObjectByType<InputManager>();
        input.CreateInputMap();

        OnValidate();
    }

    private void Update()
    {
        SetDesiredVelocity();
        desiredJump |= input.Jump.WasPressedThisFrame();
    }

    private void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(rb.position, out upAxis);

        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            bool performedJump = Jump(gravity);
            // Keep the desired to jump if the jump wasn't performed and the button press happend late
            desiredJump = !performedJump && stepsSinceLastJumped > 50;
        }

        velocity += gravity * Time.deltaTime;        
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
