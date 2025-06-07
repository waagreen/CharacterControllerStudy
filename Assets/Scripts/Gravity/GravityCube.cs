using System;
using UnityEngine;

public class GravityCube : GravitySource
{
    [SerializeField] private float force = 9.81f;
    [SerializeField] private Vector3 boundaryDistance = Vector3.zero;
    [Min(0f)][SerializeField] private float innerDistance = 0f, innerFalloffDistance = 0f;

    private float innerFalloffFactor = 0f;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        boundaryDistance = Vector3.Max(boundaryDistance, Vector3.zero);

        // Inner distances must be always smaller than the smallest component of the boundary distance vector
        float maxInner = Mathf.Min(Mathf.Min(boundaryDistance.x, boundaryDistance.y), boundaryDistance.z);

        innerDistance = Mathf.Min(maxInner, innerDistance);

        // Inner falloff must be at least as big as the inner distance
        innerFalloffDistance = Mathf.Max(Mathf.Min(maxInner, innerFalloffDistance), innerDistance);

        innerFalloffFactor = 1f / (innerFalloffDistance - innerDistance);
    }

    public int CheckSmallestDistance(Vector3 distances)
    {
        if (distances.x < Mathf.Min(distances.y, distances.z)) return 0;
        else if (distances.y < Mathf.Min(distances.x, distances.z)) return 1;
        else return 2;
    }

    private float GetGravityComponent(float coordinate, float distance)
    {
        if (distance > innerFalloffDistance) return 0f;

        float g = force;
        if (distance > innerDistance)
        {
            g *= 1f - (distance - innerDistance) * innerFalloffFactor;
        }

        return coordinate > 0f ? g : -g;
    }

    public override Vector3 GetGravity(Vector3 position)
    {
        position = transform.InverseTransformDirection(position - transform.position);
        Vector3 vector = Vector3.zero;

        Vector3 distances;
        distances.x = boundaryDistance.x - Mathf.Abs(position.x);
        distances.y = boundaryDistance.y - Mathf.Abs(position.y);
        distances.z = boundaryDistance.z - Mathf.Abs(position.z);

        // Gravity force relative to the nearest face
        int smallestFlag = CheckSmallestDistance(distances);

        switch (smallestFlag)
        {
            case 0 :
                vector.x = GetGravityComponent(position.x, distances.x);
                break;
            case 1:
                vector.y = GetGravityComponent(position.y, distances.y);
                break;
            case 2:
                vector.z = GetGravityComponent(position.z, distances.z);
                break;
        };

        return transform.TransformDirection(vector);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Vector3 size = Vector3.zero;
        if (innerFalloffDistance > innerDistance)
        {
            Gizmos.color = Color.cyan;
            size.x = 2f * (boundaryDistance.x - innerFalloffDistance);
            size.y = 2f * (boundaryDistance.y - innerFalloffDistance);
            size.z = 2f * (boundaryDistance.z - innerFalloffDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        if (innerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            size.x = 2f * (boundaryDistance.x - innerDistance);
            size.y = 2f * (boundaryDistance.y - innerDistance);
            size.z = 2f * (boundaryDistance.z - innerDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, 2f * boundaryDistance);
    }
}
