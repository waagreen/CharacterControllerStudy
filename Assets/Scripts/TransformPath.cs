using UnityEditor;
using UnityEngine;

public class TransformPath : MonoBehaviour
{
    [Min(1)][SerializeField] private int initialPoint = 1;
    protected int waypointIndex;

    private void OnValidate()
    {
        initialPoint = Mathf.Min(transform.childCount, initialPoint);
    }

    public void InitializePath()
    {
        OnValidate();
        waypointIndex = initialPoint - 1;
    }

    public Vector3 GetCurrentPoint()
    {
        return transform.GetChild(waypointIndex).position;
    }

    public Vector3 GetNextPoint()
    {
        waypointIndex += 1;
        if (waypointIndex >= transform.childCount) waypointIndex = 0;

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
            Gizmos.color = (order != initialPoint) ? Color.magenta : Color.yellow;
            Gizmos.DrawSphere(child.position, 0.15f);
            Handles.Label(child.position + Vector3.up * 0.3f, order.ToString(), style);
            order++;
        }
    }
}
