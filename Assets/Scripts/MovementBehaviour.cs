using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class MovementBehaviour : MonoBehaviour
{
    [Header("Movement")]
    [Range(0.1f, 50f)][SerializeField] private float maxSpeed;
    [Range(0.1f, 50f)][SerializeField] private float jumpForce;
    [Range(0.1f, 150f)][SerializeField] private float acceleration;
    [Range(0.1f, 150f)][SerializeField] private float deceleration;
    [Range(0.1f, 150f)][SerializeField] private float rotationSpeed;
    [Range(1f, 10f)][SerializeField] private float turnDecelerationMultiplier = 3f;
    [Min(0.1f)][SerializeField] private float maxJumpTime;


    [Space(10f)]
    [Header("Gravity Forces")]
    [SerializeField] private float ascentGravity;
    [SerializeField] private float airborneGravity;
    [SerializeField] private float terminalVelocity = 53f;
    [SerializeField] private float airResistance = 0.1f;

    [Space(10f)]
    [Header("Collision")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask hurtLayer;

    private Rigidbody rb;
    private CapsuleCollider col;
    private static bool isSet = false;

    public float MaxSpeed => maxSpeed;
    public float JumpForce => jumpForce;
    public float Acceleration => acceleration; 
    public float Deceleration => deceleration; 
    public float RotationSpeed => rotationSpeed; 
    public float AscentGravity => ascentGravity; 
    public float AirborneGravity => airborneGravity; 
    public float TerminalVelocity => terminalVelocity; 
    public float AirResistance => airResistance; 
    public float TurnDecelerationMultiplier => turnDecelerationMultiplier; 
    public float MaxJumpTime => maxJumpTime; 
    public Rigidbody Rb => rb; 

    public void Setup()
    {
        if (isSet) return;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        isSet = true;
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, col.height + 0.1f, groundLayer);
    }
}
