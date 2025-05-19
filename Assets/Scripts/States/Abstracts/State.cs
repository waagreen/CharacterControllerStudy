using UnityEngine;

public abstract class State
{
    protected StateMachine parentMachine;
    protected InputManager input;
    protected MovementBehaviour character;

    public State(StateMachine machine)
    {
        parentMachine = machine;
        input = parentMachine.InputManager;
        character = parentMachine.Character;
    }

    public virtual void ConstantBehaviour() {}
    public virtual void Enter() {}
    public virtual void Exit() {}
    public virtual void CheckTransition() {}
}
