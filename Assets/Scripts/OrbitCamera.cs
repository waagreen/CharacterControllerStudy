using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [Header("Automatic Movement")]
    [SerializeField] private Transform focus = default;
    [Range(1f, 20f)][SerializeField] private float distance = 5f;
    [Min(0f)][SerializeField] private float focusRadius = 1f;
    [Range(0f, 1f)][SerializeField] private float focusCentering = 0.5f;
    [Min(0f)][SerializeField] private float alignDelay = 5f;
    [Min(0f)][SerializeField] private float upAlignmentSpeed = 360f;

    [Header("Manual movement")]
    [Range(1f, 360f)][SerializeField] private float rotationSpeed = 90f;
    [Range(-89f, 89f)][SerializeField] private float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    [Range(0f, 90f)][SerializeField] private float alignSmoothRange = 45f;
    [SerializeField] private LayerMask obstructionMask = -1;

    private const float e = 0.001f;
    private const float kMinAutoAlignMagnitude = 0.0001f;

    // Assigned on awake (don't change)
    private InputManager input;
    private Camera regularCamera;

    // Runtime variables
    private Vector3 focusPoint, previousFocusPoint = Vector3.zero;
    private Vector3 cameraHalfExtends;
    private Vector2 orbitAngles = new(45f, 0f); // Starts looking 45f degrees down in the x axis
    private Quaternion gravityAlignment = Quaternion.identity;
    private Quaternion orbitRotation;
    private float lastManualRotationTime;

    private float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle; 
    }

    private Vector3 CalculateHalfExtends()
    {
        Vector3 halfExtends;

        halfExtends.x = regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
        halfExtends.y = halfExtends.x * regularCamera.aspect;
        halfExtends.z = 0f;

        return halfExtends;
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
        // Invert the horizontal axis for a more natural right stick movement
        Vector2 direction = new(-input.Look.y, input.Look.x);

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
            Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * (focusPoint - previousFocusPoint);

            // Automatic rotation is bound to the XZ gravity plane
            Vector2 movement = new(alignedDelta.x, alignedDelta.z);
            float movementDeltaSqr = movement.sqrMagnitude;

            // Ignore insignificant movements
            if (movementDeltaSqr < kMinAutoAlignMagnitude) return false;

            // Doing the movement normalization by hand since we already have the sqrMagnitude
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
            // Calculate t for a smooth ease out follow
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

    private void SetGravityAlignment()
    {
        Vector3 fromUp = gravityAlignment * Vector3.up;
        Vector3 toUp = CustomGravity.GetUpAxis(focusPoint);

        Quaternion newAlignment = Quaternion.FromToRotation(fromUp, toUp) * gravityAlignment;

        float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        float maxAngle = upAlignmentSpeed * Time.deltaTime;

        if (angle <= maxAngle)
        {
            gravityAlignment = newAlignment;
        }
        else
        {
            gravityAlignment = Quaternion.SlerpUnclamped(gravityAlignment, newAlignment, maxAngle / angle);
        }   
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
        regularCamera = GetComponent<Camera>();

        focusPoint = focus.position;
        transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);

        cameraHalfExtends = CalculateHalfExtends();
    }

    private void LateUpdate()
    {
        SetGravityAlignment();
        UpdateFocusPoint();

        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            orbitRotation = Quaternion.Euler(orbitAngles);
        }

        // Look rotation is always relative to the current gravity plane
        Quaternion lookRotation = gravityAlignment * orbitRotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;

        // Calculate vectors to box cast from ideal focus point
        Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
        Vector3 rectPositon = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPositon - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        // Reposition if some geometry is detected between the camera's near plane and focal point
        if (Physics.BoxCast(castFrom, cameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask))
        {
            rectPositon = castFrom + castDirection * hit.distance;
            lookPosition = rectPositon - rectOffset;
        }

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
}
