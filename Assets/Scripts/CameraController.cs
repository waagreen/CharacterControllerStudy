using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private bool shouldFollow;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float rotationSpeed = 1f;

    private bool isRotating;
    private Vector3 velocity;
    private InputManager input;

    private void Start()
    {
        input = FindFirstObjectByType<InputManager>();
        isRotating = false;
    }

    private void HandleCameraRotation()
    {
        Vector3 direction = input.Look.normalized;

        if (direction.magnitude < 0.1f)
        {
            isRotating = false;
            return;
        }
        else isRotating = true;

        float targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.RotateAround(target.position, Vector3.up, targetAngle * Time.deltaTime);
    }

    private void FollowTarget()
    {
        if (!shouldFollow || isRotating) return;

        Vector3 targetPosition = target.TransformPoint(followOffset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.LookAt(target);
    }

    private void LateUpdate()
    {
        FollowTarget();
        HandleCameraRotation();
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(target.TransformPoint(followOffset), 1f);        
    }
}
