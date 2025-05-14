using UnityEngine;

public class Jump : State
{
    public Jump(StateMachine machine) : base(machine)
    {
    }

    public override void Enter()
    {
        Vector2 upwardsVelocity = Vector2.up * character.JumpForce;
        character.Rigidbody.AddForce(upwardsVelocity, ForceMode.Impulse);
    }
}
