using UnityEngine;

public class Airborne : FreeControl
{
    public Airborne(StateMachine machine) : base(machine)
    {
    }

    protected override float ContextualGravity => character.AirborneGravity;

    public override void CheckTransition()
    {
        base.CheckTransition();
        if (character.IsGrounded()){ parentMachine.ChangeSuperState(Verb.Grounded); Debug.Log("Going back to grounded");}
    }
}
