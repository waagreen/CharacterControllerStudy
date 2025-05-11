using UnityEngine;

public abstract class State
{
    public abstract void Behaviour();
    public abstract void Enter();
    public abstract void Exit();
    public abstract void CheckTransition();
}
