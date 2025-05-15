using UnityEngine;

public class Jump : State
{
    public Jump(StateMachine machine) : base(machine)
    {
    }

    public override void Enter()
    {
        Vector2 upwardsVelocity = Vector2.up * character.JumpForce;
        character.Rb.AddForce(upwardsVelocity, ForceMode.Impulse);
        parentMachine.ChangeSuperState(Verb.Airbonrne);
    }
}
