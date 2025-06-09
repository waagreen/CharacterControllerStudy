using System;
using UnityEngine;

public class GravityCube : GravitySource
{
    [SerializeField] private float force = 9.81f;
    [Header("Bounds settings")]
    [SerializeField] private bool constrainProportions;
    [SerializeField] private Vector3 boundaryDistance = Vector3.zero;
    [Min(0f)][SerializeField] private float innerDistance = 0f, innerFalloffDistance = 0f;
    [Min(0f)][SerializeField] private float outerDistance = 0f, outerFalloffDistance = 0f;

    private float innerFalloffFactor, outerFalloffFactor;
    private const float kSqrtOfThird = 0.5773502692f;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        boundaryDistance = Vector3.Max(boundaryDistance, Vector3.zero);

        if (constrainProportions)
        {
            // When proportions are constrained, use the average of all components for all axes
            // This allows both increasing and decreasing size while maintaining proportions
            float average = (boundaryDistance.x + boundaryDistance.y + boundaryDistance.z) / 3f;
            average = Mathf.Round(average * 100f) / 100f;
            boundaryDistance = new(average, average, average);
        }

        // Inner distances must be always smaller than the smallest component of the boundary distance vector
        float maxInner = Mathf.Min(Mathf.Min(boundaryDistance.x, boundaryDistance.y), boundaryDistance.z);

        innerDistance = Mathf.Min(maxInner, innerDistance);

        // Inner falloff must be at least as big as the inner distance
        innerFalloffDistance = Mathf.Max(Mathf.Min(maxInner, innerFalloffDistance), innerDistance);
        outerFalloffDistance = Mathf.Max(outerFalloffDistance, outerDistance);

        innerFalloffFactor = 1f / (innerFalloffDistance - innerDistance);
        outerFalloffFactor = 1f / (outerFalloffDistance - outerDistance);
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

        return coordinate < 0f ? -g : g;
    }

    private Vector3 TryToAdjustOutsideBoundsVector(Vector3 position, out int outsideCount)
    {
        Vector3 vector = Vector3.zero;
        outsideCount = 0;

        // X-Bounds
        if (position.x > boundaryDistance.x)
        {
            vector.x = boundaryDistance.x - position.x;
            outsideCount++;
        }
        else if (position.x < -boundaryDistance.x)
        {
            vector.x = -boundaryDistance.x - position.x;
            outsideCount++;
        }

        // Y-Bounds
        if (position.y > boundaryDistance.y)
        {
            vector.y = boundaryDistance.y - position.y;
            outsideCount++;
        }
        else if (position.y < -boundaryDistance.y)
        {
            vector.y = -boundaryDistance.y - position.y;
            outsideCount++;
        }

        // Z-Bounds
        if (position.z > boundaryDistance.z)
        {
            vector.z = boundaryDistance.z - position.z;
            outsideCount++;
        }
        else if (position.z < -boundaryDistance.z)
        {
            vector.z = -boundaryDistance.z - position.z;
            outsideCount++;
        }

        return vector;
    }

    public override Vector3 GetGravity(Vector3 position)
    {
        position = transform.InverseTransformDirection(position - transform.position);

        // While going beyond the outside constant gravity range, treat the force as if we were on a sphere 
        Vector3 vector = TryToAdjustOutsideBoundsVector(position, out int outsideCount);
        if (outsideCount > 0)
        {
			float distance = vector.magnitude;
			if (distance > outerFalloffDistance) return Vector3.zero;

            float g = force / distance;
			if (distance > outerDistance)
            {
				g *= 1f - (distance - outerDistance) * outerFalloffFactor;
			}
			return transform.TransformDirection(g * vector);
        }

        Vector3 distances;
        distances.x = boundaryDistance.x - Mathf.Abs(position.x);
        distances.y = boundaryDistance.y - Mathf.Abs(position.y);
        distances.z = boundaryDistance.z - Mathf.Abs(position.z);

        // Gravity force relative to the nearest face
        switch (CheckSmallestDistance(distances))
        {
            case 0:
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

    private void DrawGizmosRect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }

    private void DrawGizmosOuterCube(float distance)
    {
        for (int axis = 0; axis < 3; axis++)
        {
            DrawExtendedFace(axis, 1, distance);  // Positive face
            DrawExtendedFace(axis, -1, distance); // Negative face
        }
        
        distance *= kSqrtOfThird;
        Vector3 outerSize = 2f * (boundaryDistance + Vector3.one * distance);
        Gizmos.DrawWireCube(Vector3.zero, outerSize);
    }

    private void DrawExtendedFace(int axis, int direction, float distance)
    {
        Vector3 center = Vector3.zero;
        center[axis] = (boundaryDistance[axis] + distance) * direction;
        
        Vector3 halfSize = boundaryDistance;
        Vector3 size = 2f * halfSize;
        size[axis] = 0f;
        
        int axis1 = (axis + 1) % 3;
        int axis2 = (axis + 2) % 3;
        
        Vector3 a = center, b = center, c = center, d = center;
        a[axis1] += halfSize[axis1]; a[axis2] += halfSize[axis2];
        b[axis1] += halfSize[axis1]; b[axis2] -= halfSize[axis2];
        c[axis1] -= halfSize[axis1]; c[axis2] -= halfSize[axis2];
        d[axis1] -= halfSize[axis1]; d[axis2] += halfSize[axis2];
        
        DrawGizmosRect(a, b, c, d);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Vector3 size = Vector3.zero;

        Gizmos.color = Color.cyan;
        if (innerFalloffDistance > innerDistance)
        {
            size.x = 2f * (boundaryDistance.x - innerFalloffDistance);
            size.y = 2f * (boundaryDistance.y - innerFalloffDistance);
            size.z = 2f * (boundaryDistance.z - innerFalloffDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
        if (outerFalloffDistance > outerDistance)
        {
            Gizmos.color = Color.cyan;
            DrawGizmosOuterCube(outerFalloffDistance);
        }

        Gizmos.color = Color.yellow;
        if (innerDistance > 0f)
        {
            size.x = 2f * (boundaryDistance.x - innerDistance);
            size.y = 2f * (boundaryDistance.y - innerDistance);
            size.z = 2f * (boundaryDistance.z - innerDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
        if (outerDistance > 0f)
        {
            DrawGizmosOuterCube(outerDistance);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, 2f * boundaryDistance);
    }
}
