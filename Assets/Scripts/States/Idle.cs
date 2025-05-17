using UnityEngine;

public class Idle : State
{
    public Idle(StateMachine machine) : base(machine)
    {
    }

    public override void CheckTransition()
    {
        base.CheckTransition();

        if (input.Movement != Vector2.zero) parentMachine.ChangeSubState(Verb.Moving);
    }
}
