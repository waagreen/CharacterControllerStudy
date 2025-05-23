using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform focus = default;
    [Range(1f, 20f)][SerializeField] private float distance = 5f;
    [Min(0f)][SerializeField] private float focusRadius = 1f;
    [Range(0f, 1f)][SerializeField] private float focusCentering = 0.5f;
    [Range(1f, 360f)][SerializeField] private float rotationSpeed = 90f;
    [Range(-89f, 89)][SerializeField] private float minVerticalAngle = -30f, maxVerticalAngle = 60f;

    private const float e = 0.001f;

    // Assigned on awake (don't change)
    private InputManager input;

    // Runtime variables
    private Vector3 focusPoint;
    private Vector2 orbitAngles = new(45f, 0f); // Starts looking 45f degrees down in the x axis

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
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * direction;
            return true;
        }

        return false;
    }

    private void UpdateFocusPoint()
    {
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
        if (ManualRotation())
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
