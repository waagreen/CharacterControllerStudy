using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Chaser : MovingBody
{
    [Tooltip("Time in seconds that will be waited before the chase starts.")][SerializeField][Min(0f)] private float startCooldown = 5f;

    private Transform target;
    private Rigidbody body;
    private Vector3 previousPoint, nextPoint;
    private Queue<Vector3> path;
    private float startDelta;
    private int pathBuffer;
    private bool InterpolateMovement => startDelta >= startCooldown;
    private const int capacityPerSecond = 60;

    public Vector3 LastPoint => nextPoint;

    public void SetTarget(Transform target)
    {
        body = GetComponent<Rigidbody>();
        this.target = target;

        pathBuffer = Mathf.Max(1, (int)(capacityPerSecond * startCooldown));
        path = new(pathBuffer);

        previousPoint = body.position;
        nextPoint = target.position;
    }

    private void HandlePath()
    {
        path.Enqueue(target.position);

        if (!InterpolateMovement) return;

        previousPoint = nextPoint;
        nextPoint = path.Dequeue();
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        
        float deltaTime = Time.deltaTime;

        if (!InterpolateMovement) startDelta += deltaTime;

        HandlePath();

        body.MovePosition(Vector3.Lerp(previousPoint, nextPoint, deltaTime));
    }
}
