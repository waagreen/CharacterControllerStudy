using UnityEditor;
using UnityEngine;

public class TransformPath : MonoBehaviour
{
    [Min(1)][SerializeField] private int initialPoint = 1;
    [SerializeField] private bool reverse = false;

    protected int waypointIndex;
    protected int direction;

    private void OnValidate()
    {
        initialPoint = Mathf.Min(transform.childCount, initialPoint);
        direction = reverse ? -1 : 1;
    }

    public void InitializePath()
    {
        OnValidate();

        waypointIndex = initialPoint - 1;
    }

    public Transform GetCurrentPoint()
    {
        return transform.GetChild(waypointIndex);
    }

    public Transform GetNextPoint()
    {
        int next = waypointIndex + direction;
        if ((next >= transform.childCount) || (next < 0))
        {
            direction *= -1;
        }

        waypointIndex += direction;

        return GetCurrentPoint();
    }

    private void OnDrawGizmosSelected()
    {
        int order = 1;
        GUIStyle style = new()
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            richText = true
        };

        foreach (Transform child in transform)
        {
            if (child == null) continue;
            Gizmos.color = (order != initialPoint) ? Color.magenta : Color.yellow;
            Gizmos.DrawSphere(child.position, 0.15f);
            Handles.Label(child.position + Vector3.up * 0.3f, order.ToString(), style);
            order++;
        }
    }
}
