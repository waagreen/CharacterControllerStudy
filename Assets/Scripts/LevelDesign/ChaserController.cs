using System.Collections.Generic;
using UnityEngine;

public class ChaserController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody chaser;
    [SerializeField][Min(0f)] private float startCooldown = 5f, pointsCooldown = 1f, movementDuration;
    [SerializeField][Min(1)] private int pathCapacity = 50;

    private Queue<Vector3> path;
    private Vector3 previousPoint;
    private Vector3 nextPoint;
    private float value;
    private float pointsDelta;
    private float startDelta;
    private bool InterpolateMovement => startDelta >= startCooldown;

    public Vector3 LastPoint => nextPoint;

    private void Awake()
    {
        path = new(pathCapacity);
        previousPoint = chaser.position;
        nextPoint = target.position;
    }

    private void HandlePath()
    {
        if (pointsDelta >= pointsCooldown)
        {
            pointsDelta = 0f;
            path.Enqueue(target.position);
        }

        if (!InterpolateMovement) return;

        value += Time.deltaTime / movementDuration;
        if (value >= 1f)
        {
            value = 0f;
            previousPoint = nextPoint;
            nextPoint = path.Dequeue();
        }

    }

    private void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;

        if (!InterpolateMovement) startDelta += deltaTime;
        pointsDelta += deltaTime;

        HandlePath();

        chaser.MovePosition(Vector3.LerpUnclamped(previousPoint, nextPoint, value));
    }
}
