using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Character : MonoBehaviour
{
    [Header("Movement")]
    [Range(0f, 50f)][SerializeField] private float speed;
    [Range(0f, 15f)][SerializeField] private float jumpForce;
    [Range(0f, 50f)][SerializeField] private float acceleration;
    [Range(0f, 50f)][SerializeField] private float deceleration;

    [Space(10f)]
    [Header("Collision")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask hurtLayer;

    public float Speed 
    {
        get => speed;
    }
    
    public float JumpForce
    {
        get => jumpForce;
    }

    public float Acceleration
    {
        get => acceleration;
    }

    public float Deceleration
    {
        get => deceleration;
    }

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
        return Physics.Raycast(transform.position, Vector3.down, col.height + 0.2f, groundLayer);
    }
}
