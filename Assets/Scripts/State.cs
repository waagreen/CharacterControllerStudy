using UnityEngine;

public abstract class State
{
    protected StateMachine parentMachine;
    protected InputManager input;
    protected Character character;


    public State(StateMachine machine)
    {
        parentMachine = machine;
        input = parentMachine.InputManager;
        character = parentMachine.Character;
    }

    public abstract void Behaviour();
    public abstract void Enter();
    public abstract void Exit();
    public abstract void CheckTransition();
}
