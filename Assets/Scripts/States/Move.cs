using UnityEngine;

public class Move : State
{
    private Vector3 velocity = Vector3.zero;

    public Move(StateMachine machine) : base(machine)
    {
    }

    public override void Behaviour()
    {
        // Make a vector using y as forward instead of up
        Vector3 direction = new(input.Movement.x, 0f, input.Movement.y); 
        
        // Linear interpolate between idle and moving based on input magnitute
        Vector3 lerpedDirection = Vector3.Lerp(Vector3.zero, direction.normalized * character.Speed, input.Movement.magnitude);
        
        // Acceleration and deceleration are interpolated so we can have different values for them
        float rateOfChange = Mathf.Lerp(character.Deceleration, character.Acceleration, direction.magnitude);
     
        // Finally directly modify the character velocity interpolating with the previous velocity
        velocity = Vector3.Lerp(character.Rigidbody.linearVelocity, lerpedDirection, rateOfChange * Time.deltaTime);
        character.Rigidbody.linearVelocity = velocity;
    }

    public override void CheckTransition()
    {
        if ((input.Movement == Vector2.zero) && (velocity == Vector3.zero))
        {
            parentMachine.ChangeState(Verb.Idling);
        }
    }

    private void GoToJump()
    {
        Debug.Log(character.IsGrounded());
        if (!character.IsGrounded()) return;

        parentMachine.ChangeState(Verb.Jumping);
    }

    public override void Enter()
    {
        input.OnJump += GoToJump;
    }

    public override void Exit()
    {
        input.OnJump -= GoToJump;
    }
}
