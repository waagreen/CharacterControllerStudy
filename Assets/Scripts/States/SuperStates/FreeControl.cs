using UnityEngine;

public class FreeControl : SuperState
{
    public FreeControl(StateMachine machine) : base(machine)
    {
    }

    protected override Verb InitialSubstate { get => Verb.Idling; }
    protected virtual float ContextualGravity { get; }

    private void HandleGravity()
    {
        Vector3 gravitationalForce = ContextualGravity * Time.deltaTime * Vector3.down;
        Debug.Log("Contextual gravity: " + ContextualGravity);
        character.Rigidbody.linearVelocity += gravitationalForce;
    }

    public override void CheckTransition()
    {
        base.CheckTransition();
        
        if ((input.Movement == Vector2.zero) && (character.Rigidbody.linearVelocity == Vector3.zero))
        {
            parentMachine.ChangeSubState(Verb.Idling);
        }
        else parentMachine.ChangeSubState(Verb.Moving);
    }

    public override void ConstantBehaviour()
    {
        base.ConstantBehaviour();
        HandleGravity();
    }
}
