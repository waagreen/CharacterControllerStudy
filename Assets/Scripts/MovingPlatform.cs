using System;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private TransformPath path;
    [SerializeField][Min(0f)] private float speed = 15f;

    private float timeToNextPoint;
    private float elapsedTime;

    private Vector3 targetPosition;
    private Vector3 currentPosition;

    private void SetWaypoints()
    {
        currentPosition = path.GetCurrentPoint();
        targetPosition = path.GetNextPoint();

        Debug.Log("current position: " + currentPosition.y);
        Debug.Log("target position: " + targetPosition.y);

        elapsedTime = 0f;

        float distance = Vector3.Distance(currentPosition, targetPosition);
        timeToNextPoint = distance / speed;
    }

    private void Start()
    {
        path.InitializePath();
        SetWaypoints();
        transform.position = currentPosition;
    }

    private void FixedUpdate()
    {
        elapsedTime += Time.deltaTime;

        float t = elapsedTime / timeToNextPoint;
        t = Mathf.SmoothStep(0, 1, t);
        transform.position = Vector3.Lerp(currentPosition, targetPosition, t);

        if (t >= 1f) SetWaypoints();
    }
}
