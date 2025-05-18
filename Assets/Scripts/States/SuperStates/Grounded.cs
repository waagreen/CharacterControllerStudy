using UnityEngine;

public class Grounded : SuperState
{
    public Grounded(StateMachine machine) : base(machine)
    {
    }

    protected override Verb InitialSubstate { get => Verb.Idling; }

    private void ReactToJumpInput(bool flag)
    {
        if (!flag) return;
        parentMachine.ChangeSubState(Verb.Jumping);
    }

    public override void CheckTransition()
    {
        base.CheckTransition();

        // TODO: FIX GROUNDED GRAVITY
        // if (!character.IsGrounded())
        // {
        //     parentMachine.ChangeSuperState(Verb.Airbonrne);
        // }
    }

    public override void Enter()
    {
        base.Enter();
        input.OnJump += ReactToJumpInput;
    }

    public override void Exit()
    {
        input.OnJump -= ReactToJumpInput;
        base.Exit();
    }

}
