using UnityEngine;

public class Grounded : FreeControl
{
    public Grounded(StateMachine machine) : base(machine)
    {
    }

    private void GoToJump()
    {
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
