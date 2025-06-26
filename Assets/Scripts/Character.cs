using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Character : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform playerInputSpace;
    [Range(0f, 100f)][SerializeField] private float maxSpeed = 10f, maxClimbSpeed = 5f, maxSnapSpeed = 11f, maxSwimSpeed = 5f;
    [Range(0f, 100f)][SerializeField] private float maxAcceleration = 20f, maxClimbAcceleration = 60f, maxAirAcceleration = 1f, maxSwimAcceleration = 5f;
    [Range(0f, 90f)][SerializeField] private float maxGroundAngle = 25f, maxStairAngle = 46f;
    [Range(90f, 180f)][SerializeField] private float maxClimbAngle = 140f;
    [Range(0f, 10f)][SerializeField] private float waterDrag = 1f;
    [Min(0f)][SerializeField] private float buoyancy = 1f;
    [Range(0.1f, 1f)][SerializeField] private float swimThreshold = 0.5f;

    [Header("Jump Settings")]
    [Range(1f, 10f)][SerializeField] private float jumpHeight = 2f;
    [Range(0, 5)][SerializeField] private int maxAirJumps = 2;

    [Header("Raycast Settings")]
    [Min(0f)][SerializeField] private float probeDistance = 1f;
    [SerializeField] private float submergeOffset = 0.5f;
    [Min(0.1f)][SerializeField] private float submergeRange = 1f;
    [SerializeField] private LayerMask probeMask = -1, stairMask = -1, climbMask = -1, waterMask = 0;

    [Header("Debug Visuals")]
    [SerializeField] private bool displayDebugVisuals = true;
    [SerializeField] private Material groundMat, climbMat, swimmingMat = default;

    // Assigned on awake (don't change)
    private InputManager input;
    private Rigidbody rb;
    private MeshRenderer rend;

    // Runtime variables
    private Rigidbody connectedBody, previousConnectedBody;
    private Vector3 velocity, connectionVelocity = Vector3.zero;
    private Vector3 contactNormal, steepNormal, climbNormal, lastClimbNormal = Vector3.zero;
    private Vector3 upAxis, rightAxis, forwardAxis;
    private Vector3 connectionWorldPosition, connectionLocalPosition;
    private int groundContactCount, steepContactCount, climbContactCount = 0;
    private int stepsSinceLastGrounded, stepsSinceLastJumped = 0;
    private float minGroundDotProduct, minStairDotProduct, minClimbDotProduct = 0f;
    private float submergence;
    private int jumpPhase = 0;
    private bool desiresJump, desiresClimbing = false;

    private bool Swimming => submergence >= swimThreshold;
    private bool InWater => submergence > 0;
    private bool Grounded => groundContactCount > 0;
    private bool OnSteep => steepContactCount > 0;
    private bool Climbing => climbContactCount > 0 && (stepsSinceLastJumped > 2); // Checking steps prevents awkward interaction between climbing and wall jumping

    private const int kJumpBufferSteps = 50;
    private const float kGripForceReduction = 0.9f;

    float GetMinDot(int layer)
    {
        bool useGroundDot = (stairMask & (1 << layer)) == 0;
        return useGroundDot ? minGroundDotProduct : minStairDotProduct;
    }

    private Vector3 GetJumpDirection()
    {
        if (Grounded)
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
        if (InWater)
        {
            jumpSpeed *= Mathf.Max(0f, 1f - submergence / swimThreshold);
        }

        // Adding up axis vector to the direction so jumping on steep surfaces provides more vertical velocity
        jumpDirection = (jumpDirection + upAxis).normalized;

        // Canceling an already existing vertical velocity prevents sequential jumps to go higher than intended
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f) jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);

        velocity += jumpDirection * jumpSpeed;
        return true;
    }

    private void EvaluateSubmergence(Rigidbody waterBody)
    {
        // Adding 1 to the raycast distance to compensate for physics update while getting out the water
        if (Physics.Raycast(rb.position + upAxis * submergeOffset, -upAxis, out RaycastHit hit, submergeRange + 1f, waterMask, QueryTriggerInteraction.Collide))
        {
            submergence = 1f - hit.distance / submergeRange;
        }
        else
        {
            // Raycast can't detect the trigger water volume while inside of it. So it's fully submerged.
            submergence = 1f;
        }

        if (Swimming) connectedBody = waterBody;
    }

    private void EvaluateCollision(Collision collision)
    {
        if (Swimming) return; // We already dealing with a water volume, skip other kinds of collision detection

        int layer = collision.gameObject.layer;
        float minDot = GetMinDot(layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);

            // Surface angle is less than what was stablished in maxGroundAngle, so it's ground.
            if (upDot >= minDot)
            {
                groundContactCount++;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            }
            else
            {
                // Steep surface angles between 91º and the maxGroundAngle.
                if (upDot > -0.01f)
                {
                    steepContactCount++;
                    steepNormal += normal;
                    if (groundContactCount < 1)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }
                // Also check for climbable surfaces
                if (desiresClimbing && (upDot >= minClimbDotProduct) && (((1 << layer) & climbMask) != 0))
                {
                    climbContactCount++;
                    climbNormal += normal;
                    lastClimbNormal = normal;
                    connectedBody = collision.rigidbody;
                }
            }
        }
    }

    private Vector3 ProjectOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    private void AdjustVelocity()
    {
        Vector3 xAxis, zAxis;
        float speed, acceleration;

        if (Climbing)
        {
            speed = maxClimbSpeed;
            acceleration = maxClimbAcceleration;

            // Change axis orientation when touching a wall
            xAxis = Vector3.Cross(contactNormal, Vector3.up);
            zAxis = Vector3.up;
        }
        else if (InWater)
        {
            float swimFactor = Mathf.Min(1f, submergence / swimThreshold);

            speed = Mathf.LerpUnclamped(maxSpeed, maxSwimSpeed, swimFactor);
            acceleration = Mathf.LerpUnclamped(Grounded ? maxAcceleration : maxAirAcceleration, maxSwimAcceleration, swimFactor);

            xAxis = ProjectOnPlane(rightAxis, contactNormal);
            zAxis = ProjectOnPlane(forwardAxis, contactNormal);
        }
        else
        {
            speed = (Grounded && desiresClimbing) ? maxClimbSpeed : maxSpeed;
            acceleration = Grounded ? maxAcceleration : maxAirAcceleration;

            xAxis = ProjectOnPlane(rightAxis, contactNormal);
            zAxis = ProjectOnPlane(forwardAxis, contactNormal);
        }

        Vector3 relativeVelocity = velocity - connectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        float maxSpeedChange = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(currentX, input.Movement.x * speed, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, input.Movement.y * speed, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

        if (Swimming)
        {
            float currentY = Vector3.Dot(relativeVelocity, upAxis);
            float newY = Mathf.MoveTowards(currentY, input.Dive * speed, maxSpeedChange);

            velocity += upAxis * (newY - currentY);
        }
    }

    private bool SnapToGround()
    {
        // Only snaps if just leaved the ground without jumping or after two physics steps after jumping
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJumped <= 2) return false;

        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed) return false;

        // Try to get the ground plane directly below 
        if (!Physics.Raycast(rb.position, -upAxis, out RaycastHit hit, probeDistance, probeMask, QueryTriggerInteraction.Ignore)) return false;

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer)) return false;

        // If all checks succeed, then we found a valid surface to snap
        groundContactCount = 1;
        contactNormal = hit.normal;
        connectedBody = hit.rigidbody;

        // Align current velocity with the new surface
        float dot = Vector3.Dot(velocity, contactNormal);
        if (dot > 0f) velocity = (velocity - contactNormal * dot).normalized * speed;

        return true;
    }

    private bool CheckSwimming()
    {
        if (Swimming)
        {
            groundContactCount = 0;
            contactNormal = upAxis;
            return true;
        }

        return false;
    }

    private bool CheckSteepContacts()
    {
        if (steepContactCount < 1) return false;

        float upDot = Vector3.Dot(upAxis, steepNormal);
        if (upDot >= minGroundDotProduct)
        {
            // One ground contact represented by the sum of all steep normals
            groundContactCount = 1;
            steepNormal.Normalize();
            contactNormal = steepNormal;
            return true;
        }

        return false;
    }

    private bool CheckClimbing()
    {
        // Assign to contact variables the climbing counterparts
        if (Climbing)
        {
            // Climb normal it's an aggregate from steep contacts
            if (climbContactCount > 1)
            {
                climbNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, climbNormal);
                // If aggregate can be considered ground, climb the last contacted surface
                if (upDot >= minGroundDotProduct) climbNormal = lastClimbNormal;
            }

            groundContactCount = 1;
            contactNormal = climbNormal;
            return true;
        }

        return false;
    }

    private void UpdateConnectionState()
    {
        if (connectedBody == null) return;
        if (!connectedBody.isKinematic && connectedBody.mass < rb.mass) return;

        // Only update the connection velocity if are dealing with the same body between physics steps
        if (connectedBody == previousConnectedBody)
        {
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }

        connectionWorldPosition = rb.position;
        // Get our current contact position on local space to also take into account rotation
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }

    private void UpdateState()
    {
        velocity = rb.linearVelocity;

        stepsSinceLastGrounded++;
        stepsSinceLastJumped++;
        
        if (CheckClimbing() || CheckSwimming() || Grounded || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;

            // Only reset jump phase if at least one physics step was performed after the jump
            if (stepsSinceLastJumped > 1) jumpPhase = 0;

            // Only normalize contact if it is an aggregate
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else contactNormal = upAxis;

        UpdateConnectionState();
    }

    private void ClearState()
    {
        groundContactCount = steepContactCount = climbContactCount = 0;
        contactNormal = steepNormal = climbNormal = Vector3.zero;
        connectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
        submergence = 0;
    }

    private void ProjectAxis()
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
    }

    private void OnValidate()
    {
        // Convert angles to radians and get their cosine to compare with normal vector
        // As a surface aproaches 90º this value gets closer to zero.
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairDotProduct = Mathf.Cos(maxStairAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        rend = GetComponent<MeshRenderer>();

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        input = FindFirstObjectByType<InputManager>();
        input.CreateInputMap();

        OnValidate();
    }

    private void Update()
    {
        ProjectAxis();

        if (Swimming)
        {
            desiresJump = false;
        }
        else
        {
            desiresJump |= input.Jump.WasPressedThisFrame();
            desiresClimbing = input.ClimbValue;
        }

        if (displayDebugVisuals)
        {
            rend.material = Climbing ? climbMat : Swimming ? swimmingMat : groundMat;
        }
        else rend.material = groundMat;
    }

    private void FixedUpdate()
    {
        // Up axis is always defined as the opposite of the current gravity vector
        Vector3 gravity = CustomGravity.GetGravity(rb.position, out upAxis);
        UpdateState();

        if (InWater)
        {
            // Water drag comes first so in extreme cases at least some amount of acceleration is still possible
            velocity *= 1f - waterDrag * submergence * Time.deltaTime;
        }

        AdjustVelocity();

        if (desiresJump)
        {
            bool performedJump = Jump(gravity);
            // Keep the desired to jump if the jump wasn't performed and the button press happend late
            desiresJump = !performedJump && stepsSinceLastJumped > kJumpBufferSteps;
        }

        if (Climbing)
        {
            // Simulating grip by applying force contrary to surface normal
            velocity -= Time.deltaTime * kGripForceReduction * maxClimbAcceleration * contactNormal;
        }
        else if (InWater)
        {
            velocity += gravity * ((1f - buoyancy * submergence) * Time.deltaTime);
        }
        else if (Grounded && velocity.sqrMagnitude < 0.1f)
        {
            // Prevents sliding when the velocity is too low
            velocity += contactNormal * (Vector3.Dot(gravity, contactNormal) * Time.deltaTime);
        }
        else if (desiresClimbing && Grounded)
        {
            // Apply extra force to slow down movement when expressing the desire to climb
            velocity += (gravity - contactNormal * (maxClimbAcceleration * kGripForceReduction)) * Time.deltaTime;
        }
        else
        {
            velocity += gravity * Time.deltaTime;
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

    private void OnTriggerEnter(Collider other)
    {
        if ((waterMask & (1 << other.gameObject.layer)) == 0) return;
        EvaluateSubmergence(other.attachedRigidbody);
    }

    private void OnTriggerStay(Collider other)
    {
        if ((waterMask & (1 << other.gameObject.layer)) == 0) return;
        EvaluateSubmergence(other.attachedRigidbody);
    }
}
