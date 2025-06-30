using UnityEngine;

public class PositionInterpolator : MonoBehaviour
{
    [SerializeField] private Rigidbody body;
    [SerializeField] private Vector3 from = default, to = default;

    private Vector3 p1;
    private Vector3 p2;

    void OnValidate()
    {
        p1 = transform.TransformPoint(from);
        p2 = transform.TransformPoint(to);
    }

    private void Awake()
    {
        OnValidate();
    }

    public void Interpolate(float t)
    {
        body.MovePosition(Vector3.LerpUnclamped(p1, p2, t));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(p1, 0.2f);
        Gizmos.DrawSphere(p2, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(p1, p2);
    }
}
