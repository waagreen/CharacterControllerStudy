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
        float gravity = character.Rb.linearVelocity.y > 0 ? character.AscentGravity : character.AirborneGravity;
        
        Vector3 gravitationalForce = gravity * Time.deltaTime * Vector3.down;
        character.Rb.linearVelocity += gravitationalForce;
        
        if (character.Rb.linearVelocity.y < -character.TerminalVelocity)
        {
            Vector3 clampedVelocity = character.Rb.linearVelocity;
            clampedVelocity.y = -character.TerminalVelocity;
            
            character.Rb.linearVelocity = clampedVelocity;
        }

        if (character.AirResistance > 0)
        {
            Vector3 horizontalVelocity = character.Rb.linearVelocity;
            horizontalVelocity.y = 0f;
            
            character.Rb.linearVelocity = Vector3.Lerp
            (
                character.Rb.linearVelocity,
                new Vector3(0f, character.Rb.linearVelocity.y, 0f),
                character.AirResistance * Time.deltaTime
            );
        }
    }

    public override void CheckTransition()
    {
        base.CheckTransition();
        if (Time.time > (enterTime + kMinAirborneTime) && character.IsGrounded())
        { 
            parentMachine.ChangeSuperState(Verb.Grounded);
        }
    }

    public override void ConstantBehaviour()
    {
        base.ConstantBehaviour();
        HandleGravity();
    }
}
