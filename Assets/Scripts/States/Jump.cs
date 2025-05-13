using UnityEngine;

public class Jump : State
{
    public Jump(StateMachine machine) : base(machine)
    {
    }

    public override void Behaviour()
    {

    }

    public override void CheckTransition()
    {

    }

    public override void Enter()
    {
        Vector2 upwardsVelocity = Vector2.up * character.JumpForce;
        character.Rigidbody.AddForce(upwardsVelocity, ForceMode.Impulse);
        parentMachine.ChangeState(Verb.Moving);
    }

    public override void Exit()
    {
    }
}
