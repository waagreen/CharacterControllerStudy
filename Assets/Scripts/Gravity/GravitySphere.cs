using UnityEngine;

public class GravitySphere : GravitySource
{
    [SerializeField] private float force = 9.81f;
    [Min(0f)][SerializeField] private float outerRadius = 10f, outerFalloffRadius = 15f;
    [Min(0f)][SerializeField] private float innerRadius = 5f, innerFalloffRadius = 1f;


    private float outerFalloffFactor, innerFalloffFactor;

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 up = transform.position - position;

        float distance = up.magnitude;
        if (distance > outerFalloffRadius || distance < innerFalloffRadius) return Vector3.zero;

        // Outer and inner radii indicates the space where gravity is constant
        float g = force / distance;
        if (distance > outerRadius)
        {
            g *= 1f - (distance - outerRadius) * outerFalloffFactor;
        }
        else if (distance < innerRadius)
        {
            g *= 1f - (innerRadius - distance) * innerFalloffFactor;
        }

        return g * up;
    }

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        innerFalloffFactor = Mathf.Max(innerFalloffRadius, 0f); // Smallest radii around the sphere center
        innerRadius = Mathf.Max(innerRadius, innerFalloffRadius);
        outerRadius = Mathf.Max(innerRadius, outerRadius);
        outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius); // Biggest raddi, the "atmosphere"

        innerFalloffFactor = 1f / (innerRadius - innerFalloffRadius);
        outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
    }

    private void OnDrawGizmos()
    {
        Vector3 p = transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, outerRadius);
        if (innerRadius > 0f && outerRadius > innerRadius) Gizmos.DrawWireSphere(p, innerRadius);

        Gizmos.color = Color.cyan;
        if (outerRadius < outerFalloffRadius) Gizmos.DrawWireSphere(p, outerFalloffRadius);
        if (innerFalloffRadius > 0f && innerRadius > innerFalloffRadius) Gizmos.DrawWireSphere(p, innerFalloffRadius);
    }
}
