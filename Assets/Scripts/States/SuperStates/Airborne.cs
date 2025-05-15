using UnityEngine;

public class Airborne : FreeControl
{
    private float enterTime = 0f;
    private const float kMinAirborneTime = 0.1f;

    public Airborne(StateMachine machine) : base(machine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enterTime = Time.time;
    }

    private void HandleGravity()
    {
        // Apply less gravity while ascending
        float gravity = character.Rb.linearVelocity.y > 0 ? character.AscentGravity : character.AirborneGravity;
        
        Vector3 gravitationalForce = gravity * Time.deltaTime * Vector3.down;
        character.Rb.linearVelocity += gravitationalForce;

        // Clamp vertical velocity to terminal velocity (falling characters shouldn't accelerate infinitely)
        if (character.Rb.linearVelocity.y < -character.TerminalVelocity)
        {
            Vector3 clampedVelocity = character.Rb.linearVelocity;
            clampedVelocity.y = -character.TerminalVelocity;

            character.Rb.linearVelocity = clampedVelocity;
        }

        // Apply horizontal air resistance (How far you can land)
        if (character.AirResistance > 0)
        {
            Vector3 horizontalVelocity = character.Rb.linearVelocity;
            horizontalVelocity.y = 0f;

            horizontalVelocity = Vector3.Lerp
            (
               horizontalVelocity,
                Vector3.zero,
                character.AirResistance * Time.deltaTime
            );

            character.Rb.linearVelocity = new(horizontalVelocity.x, character.Rb.linearVelocity.y, horizontalVelocity.z);
        }
    }

    public override void CheckTransition()
    {
        base.CheckTransition();
        if (Time.time > (enterTime + kMinAirborneTime) && character.IsGrounded())
        {
            // Kills vertical velocity when hits the ground to avoid bouncing
            Vector3 noVerticalVelocity = character.Rb.linearVelocity;
            noVerticalVelocity.y = 0;

            character.Rb.linearVelocity = noVerticalVelocity;

            parentMachine.ChangeSuperState(Verb.Grounded);
        }
    }

    public override void ConstantBehaviour()
    {
        base.ConstantBehaviour();
        HandleGravity();
    }
}
