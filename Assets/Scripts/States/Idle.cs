using UnityEngine;

public class Idle : State
{
    public Idle(StateMachine machine) : base(machine)
    {
    }

    public override void Behaviour()
    {
    }

    public override void CheckTransition()
    {
        if (input.Movement != Vector2.zero)
        {
            parentMachine.ChangeState(Verb.Moving);
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
