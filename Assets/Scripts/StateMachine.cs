using System;
using System.Collections.Generic;
using UnityEngine;

public enum Verb
{
    Idling = 0,
    Moving = 1,
    Jumping = 2,
    Grabing = 4,
    Grounded = 8,
    Airbonrne = 16
}

public class StateMachine : MonoBehaviour
{
    [SerializeField] private Verb initalSuperState;
    [SerializeField] private Character character;
    [SerializeField] private InputManager inputManager;

    public Character Character => character;
    public InputManager InputManager => inputManager;

    public State currentState = null;
    public State currentSuperState = null;
    private readonly Dictionary<Verb, State> aviableStates = new();

    public void ChangeSuperState(Verb verb)
    {
        currentSuperState?.Exit();

        currentSuperState = aviableStates[verb];
        currentSuperState.Enter();
        Debug.Log("SUPER STATE " + verb);
    }

    public void ChangeSubState(Verb verb)
    {
        currentState?.Exit();

        currentState = aviableStates[verb];
        currentState.Enter();
    }

    private State VerbToState(Verb verb) 
    {
        return verb switch
        {
            Verb.Idling => new Idle(this),
            Verb.Moving => new Move(this),
            Verb.Jumping => new Jump(this),
            Verb.Grabing => new Grab(this),
            Verb.Airbonrne => new Airborne(this),
            Verb.Grounded => new Grounded(this),
            _ => throw new NotImplementedException($"{verb} don't have a case on VerbToState method."),
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
        
        ChangeSuperState(initalSuperState);
    }

    private void Awake()
    {
        CreateStates();
        character = GetComponent<Character>();
        
        inputManager.CreateInputMap();
    }

    private void FixedUpdate()
    {
        currentState?.ConstantBehaviour();
        currentState?.CheckTransition();

        currentSuperState?.ConstantBehaviour();
        currentSuperState?.CheckTransition();
    }
}
