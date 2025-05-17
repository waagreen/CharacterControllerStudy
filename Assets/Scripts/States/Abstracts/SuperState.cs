using UnityEngine;

public abstract class SuperState : State
{
    public SuperState(StateMachine machine) : base(machine)
    {
    }

    protected abstract Verb InitialSubstate { get; }

    public override void Enter()
    {
        base.Enter();
        DefineInitalSubstate(InitialSubstate);
    }

    private void DefineInitalSubstate(Verb substate)
    {
        parentMachine.ChangeSubState(substate);
    }
}
