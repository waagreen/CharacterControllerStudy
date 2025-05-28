using UnityEngine;

public class GravityPlane : GravitySource
{
    [SerializeField] private float gravity = 9.81f;
    [Min(0f)][SerializeField] private float range = 1f;
    [SerializeField] private Vector2 gravityInfluenceArea = Vector2.one;

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 up = transform.up;

        float distance = Vector3.Dot(up, position - transform.position);
        if (distance > range) return Vector3.zero;

        float g = gravity;
        if (distance > 0f)
        {
            g *= 1f - distance / range;
        }

        return g * -up;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Vector3 size = new(gravityInfluenceArea.x, 0f, gravityInfluenceArea.y);
        Gizmos.DrawWireCube(Vector3.up * 0.25f, size);

        if (range > 0f)
        {
            Gizmos.color = Color.cyan;
            Vector3 scale = transform.localScale;
            scale.y = range;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.localRotation, scale);
            Gizmos.DrawWireCube(Vector3.up, size);
        }
    }
}
