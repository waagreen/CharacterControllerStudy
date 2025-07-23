using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Chaser : MovingBody
{
    [SerializeField][Min(0f)] private float startCooldown = 5f, timeToGetOnInitialPosition = 1f;

    private Transform target;
    private Rigidbody body;
    private Vector3 previousPoint, nextPoint, initialPoint;
    private Queue<Vector3> path;
    private float timer, offPathDelta;
    private int pathBuffer;
    private bool alignedToPath = false;
    private bool ShouldInterpolate => timer >= startCooldown;
    private const int capacityPerSecond = 60;

    public void SetTarget(Transform target)
    {
        body = GetComponent<Rigidbody>();
        this.target = target;

        pathBuffer = Mathf.Max(1, (int)(capacityPerSecond * (startCooldown + timeToGetOnInitialPosition)));
        path = new(pathBuffer);

        previousPoint = body.position;
        initialPoint = nextPoint = target.position;
    }

    private void AdvancePath()
    { 
        previousPoint = nextPoint;
        nextPoint = path.Dequeue();
    }

    private void TryAlignToPath()
    {
        if (!alignedToPath)
        {
            previousPoint = initialPoint;
            if (path.Count > 0) nextPoint = path.Dequeue();
            else nextPoint = initialPoint;
            alignedToPath = true;
        }
    }

    private void FixedUpdate()
    {
        // Increment path and timer
        path.Enqueue(target.position);
        float deltaTime = Time.deltaTime;
        timer += deltaTime;

        // Starts moving only when timer value is greater than the inital cooldown
        if (!ShouldInterpolate) return;

        bool goingToStartPosition = timer < (startCooldown + timeToGetOnInitialPosition);

        if (goingToStartPosition)
        {
            float t = deltaTime / timeToGetOnInitialPosition;
            offPathDelta += t;
            body.MovePosition(Vector3.Lerp(previousPoint, initialPoint, offPathDelta));
        }
        else
        {
            TryAlignToPath();
            body.MovePosition(Vector3.Lerp(previousPoint, nextPoint, deltaTime));
            AdvancePath();
        }
    }
}
