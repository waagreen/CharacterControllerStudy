using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    [Range(-180f, 180f)][SerializeField] private float movementAngle = 45f;
    [SerializeField] private Rigidbody platform;
    Vector3 targetRotation;

    private void FixedUpdate()
    {
        targetRotation += Time.deltaTime * movementAngle * Vector3.up;
        if (targetRotation.y >= 360f) targetRotation.y -= 360f;
        else if (targetRotation.y < 0) targetRotation.y += 360f;

        platform.MoveRotation(Quaternion.Slerp(platform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime));
    }
}
