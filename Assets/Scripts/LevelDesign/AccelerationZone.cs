using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AccelerationZone : MonoBehaviour
{

    [Min(0f)][SerializeField] private float speed = 10f;
    [Tooltip("Set this value to 0 to apply the force instantaneously.")][Min(0f)][SerializeField] private float acceleration = 0f;

    private Collider col;

    private void Accelerate(Rigidbody body)
    {
        Vector3 velocity = transform.InverseTransformDirection(body.linearVelocity);
        if (velocity.y >= speed) return;

        if (acceleration > 0f)
        {
            velocity.y = Mathf.MoveTowards(velocity.y, speed, acceleration * Time.deltaTime);
        }
        else velocity.y = speed;

        body.linearVelocity = transform.TransformDirection(velocity);
    }

    private void Start()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body == null) return;

        if (body.TryGetComponent(out Character c))
        {
            c.PreventGroundSnaping();
        }
        Accelerate(body);
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;

        if (body)
        {
            Accelerate(body);
        }
    }
}
