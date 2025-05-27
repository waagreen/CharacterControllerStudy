using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    [SerializeField] private bool floatToSleep = true;

    private Rigidbody rb;
    private float sleepDelay;

    private const float k001UnitsPerSecond = 0.0001f;

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

        rb.AddForce(CustomGravity.GetGravity(rb.position), ForceMode.Acceleration);
    }
}
