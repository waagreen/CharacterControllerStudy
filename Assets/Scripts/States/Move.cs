using UnityEngine;

public class Move : State
{
    private const float kSignificantTurn = 0.2f;

    public Move(StateMachine machine) : base(machine)
    {
    }

    private Vector3 HandleDirection()
    {
        Vector3 direction = new(input.Movement.x, 0f, input.Movement.y);

        if (direction.magnitude > 1f) direction.Normalize();
        if (direction.magnitude < 0.1f) direction = Vector3.zero;

        return direction;
    }

    private void HandleRotation()
    {
        Vector3 normalizedMovement = input.Movement.normalized;
        if (normalizedMovement.magnitude < 0.1f) return;

        float targetAngle = Mathf.Atan2(normalizedMovement.x, normalizedMovement.y) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        Quaternion interpolatedRotation = Quaternion.Slerp
        (
            character.transform.rotation,
            targetRotation,
            character.RotationSpeed * Time.deltaTime
        );

        character.transform.rotation = interpolatedRotation;
    }

    private float CalculateTurnAmount(Vector3 direction, Vector3 velocity)
    {
        if ((velocity.magnitude < 0.1f) || (direction.magnitude < 0.1f)) return 0f;

        // Calculate how much we're turning (0 = same direction, 1 = 180° turn)
        return 1f - Vector3.Dot(direction.normalized, velocity.normalized);
    }

    private void HandleMovement()
    {
        Vector3 direction = HandleDirection();

        Vector3 desiredVelocity = direction * character.MaxSpeed;
        Vector3 currentVelocity = character.Rb.linearVelocity;
        currentVelocity.y = 0;

        float turnAmount = CalculateTurnAmount(direction, currentVelocity);
        Vector3 velocityChange = desiredVelocity - currentVelocity;
        
        // Dynamic rate based on movement context
        float currentRate;
        if (direction.magnitude < 0.1f) // No input
        {
            currentRate = character.Deceleration;
        }
        else if (turnAmount > kSignificantTurn) // Significant turn
        {
            // Blend between acceleration and deceleration based on turn amount
            float turnSharpness = Mathf.InverseLerp(kSignificantTurn, 1f-kSignificantTurn, turnAmount);
            currentRate = Mathf.Lerp
            (
                character.Acceleration,
                character.Deceleration * character.TurnDecelerationMultiplier,
                turnSharpness
            );
        }
        else // Normal movement
        {
            currentRate = character.Acceleration * direction.magnitude;
        }

        velocityChange = Vector3.ClampMagnitude(velocityChange, currentRate * Time.deltaTime);
        character.Rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Speed clamping
        Vector3 horizontalVel = character.Rb.linearVelocity;
        horizontalVel.y = 0f;
        if (horizontalVel.magnitude > character.MaxSpeed)
        {
            horizontalVel = horizontalVel.normalized * character.MaxSpeed;
            character.Rb.linearVelocity = new Vector3(horizontalVel.x, character.Rb.linearVelocity.y, horizontalVel.z);
        }
    }

    public override void ConstantBehaviour()
    {
        HandleMovement();
        HandleRotation();
    }
}
