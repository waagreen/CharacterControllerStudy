using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TransformPath : MonoBehaviour
{
    [Min(1)][SerializeField] private int initialPoint = 1;
    [SerializeField] private bool reverse = false;

    protected int waypointIndex;
    protected int direction;
    private List<Transform> points;

    private void OnValidate()
    {
        initialPoint = Mathf.Min(transform.childCount, initialPoint);
        direction = reverse ? -1 : 1;
    }

    public void InitializePath()
    {
        OnValidate();

        waypointIndex = initialPoint - 1;

        points = new();
        foreach (Transform child in transform) points.Add(child);
    }

    public Vector3 GetCurrentPoint()
    {
        return points[waypointIndex].position;
    }

    public Vector3 GetNextPoint()
    {
        int next = waypointIndex + direction;
        if ((next >= points.Count) || (next < 0))
        {
            direction *= -1;
        }

        waypointIndex += direction;

        return GetCurrentPoint();
    }

    private void OnDrawGizmosSelected()
    {
        if (points == null) return;

        int order = 1;
        GUIStyle style = new()
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            richText = true
        };

        foreach (Transform child in points)
        {
            if (child == null) continue;
            Gizmos.color = (order != initialPoint) ? Color.magenta : Color.yellow;
            Gizmos.DrawSphere(child.position, 0.15f);
            Handles.Label(child.position + Vector3.up * 0.3f, order.ToString(), style);
            order++;
        }
    }
}
