using UnityEngine;

public class Move : State
{

    public Move(StateMachine machine) : base(machine)
    {
    }

    private void HandleRotation()
    {
        Vector3 normalizedMovement = input.Movement.normalized;
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

    private void HandleMovement()
    {
        Vector3 direction = new(input.Movement.x, 0f, input.Movement.y);

        if (direction.magnitude > 1f) direction.Normalize();

        // Calculate desired movement force
        Vector3 desiredVelocity = direction * character.Speed;
        Vector3 currentVelocity = character.Rb.linearVelocity;
        currentVelocity.y = 0;

        Vector3 velocityChange = desiredVelocity - currentVelocity;
        velocityChange = Vector3.ClampMagnitude(velocityChange, character.Acceleration * Time.deltaTime);

        character.Rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Optional speed clamping
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

    public override void CheckTransition()
    {
        if ((input.Movement == Vector2.zero) && (character.Rb.linearVelocity == Vector3.zero))
        {
            parentMachine.ChangeSubState(Verb.Idling);
        }
    }
}
