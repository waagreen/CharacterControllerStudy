using UnityEngine;

public class Grounded : FreeControl
{
    public Grounded(StateMachine machine) : base(machine)
    {
    }

    protected override float ContextualGravity => character.GroundedGravity;

    private void GoToJump()
    {
        if (!character.IsGrounded()) return;

        parentMachine.ChangeSubState(Verb.Jumping);
    }

    public override void Enter()
    {
        base.Enter();
        input.OnJump += GoToJump;
    }

    public override void Exit()
    {
        input.OnJump -= GoToJump;
        base.Exit();
    }

}
