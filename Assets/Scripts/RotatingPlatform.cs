using UnityEngine;

public enum RotationOrientation
{
    Vertical = 1,
    Horizontal = 2
}

public class RotatingPlatform : MonoBehaviour
{
    [SerializeField] private RotationOrientation orientation = RotationOrientation.Horizontal;
    [Range(-180f, 180f)][SerializeField] private float movementAngle = 45f;
    [SerializeField] private Rigidbody platform;
    
    private float currentRotationAngle = 0f;
    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = platform.rotation;
        Vector3 currentEuler = platform.rotation.eulerAngles;
        currentRotationAngle = (orientation == RotationOrientation.Vertical) ? currentEuler.y : currentEuler.x;
    }
    
    private void FixedUpdate()
    {
        currentRotationAngle += Time.fixedDeltaTime * movementAngle;
        
        // Normalize angle without clamping (more efficient than clamping to 0-360)
        currentRotationAngle %= 360f;
        
        Quaternion targetRotation;
        if (orientation == RotationOrientation.Vertical)
        {
            targetRotation = initialRotation * Quaternion.Euler(0, currentRotationAngle, 0);
        }
        else
        {
            targetRotation = initialRotation * Quaternion.Euler(currentRotationAngle, 0, 0);
        }
        
        platform.MoveRotation(targetRotation);
    }
}