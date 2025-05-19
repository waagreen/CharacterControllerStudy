using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class Character : MonoBehaviour
{
    [Range(1f, 100f)][SerializeField] private float maxSpeed = 10f;
    [Range(1f, 100f)][SerializeField] private float maxAcceleration = 10f;
    [Range(0f, 1f)][SerializeField] private float bounciness = 0.5f;
    [SerializeField] private Rect allowedArea = new(0, 0, 10f, 10f);

    Vector3 velocity = Vector3.zero;
    private InputManager input;

    private void Start()
    {
        input = GetComponent<InputManager>();
        input.CreateInputMap();
    }

    private void Update()
    {
        Vector3 desiredVelocity = new Vector3(input.Movement.x, 0f, input.Movement.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        Vector3 displacement = velocity * Time.deltaTime;
        Vector3 newPosition = transform.position + displacement;

        // Kills velocity pointing towards the collided area 
        if (newPosition.x < allowedArea.xMin)
        {
            newPosition.x = allowedArea.xMin;
            velocity.x = -velocity.x * bounciness;
        }
        else if (newPosition.x > allowedArea.xMax)
        {
            newPosition.x = allowedArea.xMax;
            velocity.x = -velocity.x * bounciness;
        }

        if (newPosition.z < allowedArea.yMin)
        {
            newPosition.z = allowedArea.yMin;
            velocity.z = -velocity.z * bounciness;
        }
        else if (newPosition.z > allowedArea.yMax)
        {
            newPosition.z = allowedArea.yMax;
            velocity.z = -velocity.z * bounciness;
        }

        transform.position = newPosition;
    }
}
