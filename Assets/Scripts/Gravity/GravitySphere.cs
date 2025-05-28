using UnityEngine;

public class GravitySphere : GravitySource
{
    [SerializeField] private float gravity = 9.81f;
    [Min(0f)][SerializeField] private float outerRadius = 10f, outerFalloffRadius = 15f;

    private float outerFalloffFactor;

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 up = transform.position - position;

        float distance = up.magnitude;
        if (distance > outerFalloffRadius) return Vector3.zero;

        // Outer radius indicate the space where gravity is constant
        float g = gravity / distance;
        if (distance > outerRadius)
        {
            g *= 1f - (distance - outerRadius) * outerFalloffFactor;
        }

        return g * up;
    }

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);
        outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
    }

    private void OnDrawGizmos()
    {
        Vector3 p = transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, outerRadius);

        if (outerRadius > outerFalloffRadius) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(p, outerFalloffRadius);
    }
}
