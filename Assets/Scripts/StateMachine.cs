using System;
using System.Collections.Generic;
using UnityEngine;

public enum Verb
{
    Idling = 0,
    Moving = 1,
    Jumping = 2,
    Grabing = 4
}

public class StateMachine : MonoBehaviour
{
    [SerializeField] Verb initalState;

    private Character character = null;
    private State currentState = null;
    private readonly Dictionary<Verb, State> aviableStates = new();

    public void ChangeState(Verb verb)
    {
        currentState?.Exit();

        currentState = aviableStates[verb];
        currentState.Enter();
    }

    private State VerbToState(Verb verb) 
    {
        return verb switch
        {
            Verb.Idling => new Idle(),
            Verb.Moving => new Move(),
            Verb.Jumping => new Jump(),
            Verb.Grabing => new Grab(),
            _ => throw new NotImplementedException(),
        };
    }

    private void CreateStates()
    {
        // Read all values from the verbs enum and instantiate the equivalent states
        Verb[] stateVerbs = (Verb[])Enum.GetValues(typeof(Verb));
        foreach(Verb verb in stateVerbs)
        {   
            aviableStates[verb] = VerbToState(verb);
        }
        
        ChangeState(initalState);
    }

    private void Awake()
    {
        CreateStates();
        character = GetComponent<Character>();
    }

    private void FixedUpdate()
    {
        currentState?.Behaviour();
        currentState?.CheckTransition();
    }
}
