using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    [SerializeField] private bool floatToSleep = true;

    [Header("Floating Settings")]
    [Range(0f, 10f)][SerializeField] private float waterDrag = 1f;
    [Min(0f)][SerializeField] private float buoyancy = 1f;
    [SerializeField] private Vector3 buoyancyOffset = Vector3.zero;
    [SerializeField] private float submergeOffset = 0.5f;
    [Min(0.1f)][SerializeField] private float submergeRange = 1f;
    [SerializeField] private LayerMask waterMask = 0;

    private Rigidbody rb;
    private Vector3 gravity;
    private float sleepDelay;
    private float submergence;

    private const float k001UnitsPerSecond = 0.0001f;

    private void ApplyGravity()
    {
        gravity = CustomGravity.GetGravity(rb.position);
     
        // Calculate bouyancy counter force
        if (submergence > 0f)
        {
            float drag = Mathf.Max(0f, 1f - waterDrag * submergence * Time.deltaTime);
            rb.linearVelocity *= drag;
            rb.angularVelocity *= drag;

            rb.AddForceAtPosition(gravity * -(buoyancy * submergence), transform.TransformPoint(buoyancyOffset), ForceMode.Acceleration);

            submergence = 0f;
        };

        rb.AddForce(gravity, ForceMode.Acceleration);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (floatToSleep)
        {
            if (rb.IsSleeping())
            {
                sleepDelay = 0f;
                return;
            }

            if (rb.linearVelocity.magnitude < k001UnitsPerSecond)
            {
                sleepDelay += Time.deltaTime;
                if (sleepDelay >= 1) return;
            }
            else sleepDelay = 0f;
        }

        ApplyGravity();
    }

    private void EvaluateSubmergence()
    {
        Vector3 upAxis = -gravity.normalized;
        if (Physics.Raycast(rb.position + upAxis * submergeOffset, -upAxis, out RaycastHit hit, submergeRange + 1f, waterMask, QueryTriggerInteraction.Collide))
        {
            submergence = 1f - hit.distance / submergeRange;
        }
        else
        {
            submergence = 1f;
        }  
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((waterMask & (1 << other.gameObject.layer)) == 0) return;
        EvaluateSubmergence();
    }

    private void OnTriggerStay(Collider other)
    {
        if (rb.IsSleeping()) return;
        if ((waterMask & (1 << other.gameObject.layer)) == 0) return;
        EvaluateSubmergence();
    }
}
