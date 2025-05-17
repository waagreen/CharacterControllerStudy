using UnityEngine;

public class Jump : State
{
    private float enterTime;

    public Jump(StateMachine machine) : base(machine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        enterTime = Time.time;
    }

    public override void ConstantBehaviour()
    {
        base.ConstantBehaviour();

        Vector3 upwardsVelocity = character.JumpForce * Time.deltaTime * Vector3.up;
        character.Rb.linearVelocity += upwardsVelocity;
    }

    public override void CheckTransition()
    {
        base.CheckTransition();

        if (!input.JumpValue || (Time.time > (enterTime + character.MaxJumpTime)))
        {
            parentMachine.ChangeSuperState(Verb.Airbonrne);
        }

    }
}
