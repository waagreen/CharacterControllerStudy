using System;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private TransformPath path;
    [SerializeField][Min(0f)] private float speed = 15f;
    [SerializeField] Rigidbody platform;

    private float timeToNextPoint;
    private float elapsedTime;

    private Transform targetPoint;
    private Transform currentPoint;

    private void SetWaypoints()
    {
        currentPoint = path.GetCurrentPoint();
        targetPoint = path.GetNextPoint();

        elapsedTime = 0f;

        float distance = Vector3.Distance(currentPoint.position, targetPoint.position);
        timeToNextPoint = distance / speed;
    }

    private void Start()
    {
        path.InitializePath();
        SetWaypoints();
        platform.position = currentPoint.position;
    }

    private void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;

        float t = elapsedTime / timeToNextPoint;
        t = Mathf.SmoothStep(0, 1, t);
        platform.MovePosition(Vector3.Lerp(currentPoint.position, targetPoint.position, t));
        platform.MoveRotation(Quaternion.Slerp(currentPoint.rotation, targetPoint.rotation, t));

        if (t >= 1f) SetWaypoints();
    }
}
