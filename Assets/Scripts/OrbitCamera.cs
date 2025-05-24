using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform focus = default;
    [Range(1f, 20f)][SerializeField] private float distance = 5f;
    [Min(0f)][SerializeField] private float focusRadius = 1f;
    [Range(0f, 1f)][SerializeField] private float focusCentering = 0.5f;
    [Range(1f, 360f)][SerializeField] private float rotationSpeed = 90f;
    [Range(-89f, 89f)][SerializeField] private float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    [Min(0f)][SerializeField] private float alignDelay = 5f;
    [Range(0f, 90f)][SerializeField] private float alignSmoothRange = 45f;

    private const float e = 0.001f;
    private const float kMinAutoAlignMagnitude = 0.0001f;

    // Assigned on awake (don't change)
    private InputManager input;

    // Runtime variables
    private Vector3 focusPoint, previousFocusPoint = Vector3.zero;
    private Vector2 orbitAngles = new(45f, 0f); // Starts looking 45f degrees down in the x axis
    private float lastManualRotationTime;

    private float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle; 
    }

    private void ConstrainAngles()
    {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        // Ensure horizontal orbit stays in the 0-360 range
        if (orbitAngles.y < 0f) orbitAngles.y += 360f;
        else if (orbitAngles.y >= 360f) orbitAngles.y -= 360f;
    }

    private bool ManualRotation()
    {
        Vector2 direction = new(input.Look.y, input.Look.x);

        if (Mathf.Abs(direction.x) > e || Mathf.Abs(direction.y) > e)
        {
            lastManualRotationTime = Time.unscaledTime;
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * direction;
            return true;
        }

        return false;
    }

    private bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay) return false;
        else
        {
            Vector2 movement = new(focusPoint.x - previousFocusPoint.x, focusPoint.z - previousFocusPoint.z);
            float movementDeltaSqr = movement.sqrMagnitude;

            // Ignore insignificant movements
            if (movementDeltaSqr < kMinAutoAlignMagnitude) return false;

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
            float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
            float turnOverDelta = 180f - deltaAbs;
            float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);

            if (deltaAbs < alignSmoothRange)
            {
                // Scale the rotation change factor so it's less abrupt for smaller deltas
                rotationChange *= deltaAbs / alignSmoothRange;
            }
            else if (turnOverDelta < alignSmoothRange)
            {
                //Prevents abrupt alignment when going towards the camera
                rotationChange *= turnOverDelta / alignSmoothRange;
            }
            orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
            return true;
        }
    }

    private void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;

        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;

            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }

            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else focusPoint = targetPoint;
    }

    private void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }

    private void Awake()
    {
        input = FindFirstObjectByType<InputManager>();
        focusPoint = focus.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }

    private void LateUpdate()
    {
        UpdateFocusPoint();

        Quaternion lookRotation;
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles);
        }
        else lookRotation = transform.localRotation;

        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
}
