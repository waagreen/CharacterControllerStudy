using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StableFloatingRigidbody : MonoBehaviour
{
    [SerializeField] private bool floatToSleep = true;

    [Header("Floating Settings")]
    [SerializeField] private Vector3[] buoyancyOffsets = default;
    [Min(0f)][SerializeField] private float buoyancy = 1.1f;
    [SerializeField] private bool safeFloating = false;
    [SerializeField] private float submergeOffset = 0.25f;
    [Min(0.1f)][SerializeField] private float submergeRange = -.5f;
    [Range(0f, 10f)][SerializeField] private float waterDrag = 1f;
    [SerializeField] private LayerMask waterMask = 0;

    private Rigidbody rb;
    private Vector3 gravity;
    private float sleepDelay;
    private float[] submergence;

    private const float k001UnitsPerSecond = 0.0001f;

    private void ApplyGravity()
    {
        gravity = CustomGravity.GetGravity(rb.position);

        float dragFactor = waterDrag * Time.deltaTime / buoyancyOffsets.Length;
        float buoyancyFactor = -buoyancy / buoyancyOffsets.Length;

        // Calculate bouyancy counter force
        for (int i = 0; i < buoyancyOffsets.Length; i++)
        {
            if (submergence[i] > 0f)
            {
                float drag = Mathf.Max(0f, 1f - dragFactor * submergence[i]);
                rb.linearVelocity *= drag;
                rb.angularVelocity *= drag;

                rb.AddForceAtPosition(gravity * (buoyancyFactor * submergence[i]), transform.TransformPoint(buoyancyOffsets[i]), ForceMode.Acceleration);

                submergence[i] = 0f;
            };
        }

        rb.AddForce(gravity, ForceMode.Acceleration);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        submergence = new float[buoyancyOffsets.Length];
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
        Vector3 down = gravity.normalized;
        Vector3 offset = down * -submergeOffset;

        for (int i = 0; i < buoyancyOffsets.Length; i++)
        {
            Vector3 rayPoint = offset + transform.TransformPoint(buoyancyOffsets[i]);
            if (Physics.Raycast(rayPoint, down, out RaycastHit hit, submergeRange + 1f, waterMask, QueryTriggerInteraction.Collide))
            {
                submergence[i] = 1f - hit.distance / submergeRange;
            }
            else if (!safeFloating || Physics.CheckSphere(rayPoint, 0.01f, waterMask, QueryTriggerInteraction.Collide))
            {
                submergence[i] = 1f;
            }
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

    private void OnDrawGizmosSelected()
    {
        if (buoyancyOffsets == null || buoyancyOffsets.Length < 1) return;

        Gizmos.color = Color.magenta;
        foreach (Vector3 point in buoyancyOffsets)
        {
            Gizmos.DrawSphere(transform.TransformPoint(point), 0.2f);
        }
    }
}
