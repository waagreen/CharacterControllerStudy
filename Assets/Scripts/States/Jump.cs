using UnityEngine;

public class Jump : State
{
    public Jump(StateMachine machine) : base(machine)
    {
    }

    public override void Behaviour()
    {
        Vector2 upwardsVelocity = Vector2.up * character.JumpForce;
        character.Rigidbody.AddForce(upwardsVelocity, ForceMode.Impulse);
    }

    public override void CheckTransition()
    {
        throw new System.NotImplementedException();
    }

    public override void Enter()
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }
}
