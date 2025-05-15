using UnityEngine;

public class FreeControl : SuperState
{
    public FreeControl(StateMachine machine) : base(machine)
    {
    }

    protected override Verb InitialSubstate { get => Verb.Idling; }


    public override void CheckTransition()
    {
        base.CheckTransition();
        
        if ((input.Movement == Vector2.zero) && (character.Rb.linearVelocity == Vector3.zero))
        {
            parentMachine.ChangeSubState(Verb.Idling);
        }
        else parentMachine.ChangeSubState(Verb.Moving);
    }
}
