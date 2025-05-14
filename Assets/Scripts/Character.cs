using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Character : MonoBehaviour
{
    [Header("Movement")]
    [Range(0f, 50f)][SerializeField] private float speed;
    [Range(0f, 50f)][SerializeField] private float maxSpeed;
    [Range(0f, 15f)][SerializeField] private float jumpForce;
    [Range(0f, 50f)][SerializeField] private float acceleration;
    [Range(0f, 50f)][SerializeField] private float rotationSpeed;


    [Space(10f)]
    [Header("Gravity Forces")]
    [SerializeField] private float groundedGravity;
    [SerializeField] private float airborneGravity;
    [SerializeField] private float terminalVelocity = 53f;
    [SerializeField] private float airResistance = 0.1f;

    [Space(10f)]
    [Header("Collision")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask hurtLayer;

    public float Speed { get => speed; }
    public float MaxSpeed { get => maxSpeed; }
    public float JumpForce { get => jumpForce; }
    public float Acceleration { get => acceleration; }
    public float RotationSpeed { get => rotationSpeed; }
    public float GroundedGravity { get => groundedGravity; }
    public float AirborneGravity { get => airborneGravity; }

    private Rigidbody rb;
    private CapsuleCollider col;

    public Rigidbody Rigidbody => rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, col.height + 0.1f, groundLayer);
    }
}
