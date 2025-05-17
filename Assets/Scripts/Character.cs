using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Character : MonoBehaviour
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

    public float MaxSpeed { get => maxSpeed; }
    public float JumpForce { get => jumpForce; }
    public float Acceleration { get => acceleration; }
    public float Deceleration { get => deceleration; }
    public float RotationSpeed { get => rotationSpeed; }
    public float AscentGravity { get => ascentGravity; }
    public float AirborneGravity { get => airborneGravity; }
    public float TerminalVelocity { get => terminalVelocity; }
    public float AirResistance { get => airResistance; }
    public float TurnDecelerationMultiplier { get => turnDecelerationMultiplier; }
    public float MaxJumpTime { get => maxJumpTime; }
    public Rigidbody Rb { get => rb; }

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
