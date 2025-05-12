using UnityEngine;

public class Move : State
{
    public Move(StateMachine machine) : base(machine)
    {
    }

    public override void Behaviour()
    {
        Vector2 direction = input.Movement.normalized;
        Vector2 velocity = 10f * character.Speed * Time.deltaTime * direction;

        Debug.Log("moving body: " + velocity);
        character.Rigidbody.AddForce(velocity, ForceMode.Force);
    }

    public override void CheckTransition()
    {
        if (input.Movement == Vector2.zero)
        {
            parentMachine.ChangeState(Verb.Idling);
        }
    }

    public override void Enter()
    {
        Debug.Log("ENTERING MOVE");
    }

    public override void Exit()
    {
        Debug.Log("EXITING MOVE");
    }
}
