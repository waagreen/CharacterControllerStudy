using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Character : MonoBehaviour
{
    [Range(0f, 50f)][SerializeField] private float speed;
    [Range(0f, 15f)][SerializeField] private float jumpForce;

    public float Speed => speed;
    public float JumpForce => jumpForce;

    private Rigidbody rb;
    private CapsuleCollider col;

    public Rigidbody Rigidbody => rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }
}
