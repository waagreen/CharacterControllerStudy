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

    public override void Enter()
    {
        Debug.Log("ENTERING IDLE");
    }

    public override void Exit()
    {
        Debug.Log("EXITING IDLE");
    }
}
