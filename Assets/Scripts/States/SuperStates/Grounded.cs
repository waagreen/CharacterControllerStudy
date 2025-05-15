using UnityEngine;

public class Grounded : FreeControl
{
    public Grounded(StateMachine machine) : base(machine)
    {
    }

    private void GoToJump()
    {
        if (!character.IsGrounded()) return;

        parentMachine.ChangeSubState(Verb.Jumping);
    }

    public override void ConstantBehaviour()
    {
        base.ConstantBehaviour();
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
