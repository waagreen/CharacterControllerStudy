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

[RequireComponent(typeof(Character), typeof(InputManager))]
public class StateMachine : MonoBehaviour
{
    private Character character;
    private InputManager inputManager;
    private State currentState = null;
    private State currentSuperState = null;
    private readonly Dictionary<Verb, State> aviableStates = new();

    public Character Character => character;
    public InputManager InputManager => inputManager;

    public void ChangeSuperState(Verb verb)
    {
        State stateToGo = aviableStates[verb];
        if (currentSuperState == stateToGo) return;

        currentSuperState?.Exit();

        currentSuperState = stateToGo;
        currentSuperState.Enter();
    }

    public void ChangeSubState(Verb verb)
    {
        State stateToGo = aviableStates[verb];
        if (currentState == stateToGo) return;

        currentState?.Exit();

        currentState = stateToGo;
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

    private void SetInitialSuperState()
    {
        if (character.IsGrounded()) ChangeSuperState(Verb.Grounded);
        else ChangeSuperState(Verb.Airbonrne);
    }

    private void CreateStates()
    {
        // Read all values from the verbs enum and instantiate the equivalent states
        Verb[] stateVerbs = (Verb[])Enum.GetValues(typeof(Verb));
        foreach (Verb verb in stateVerbs)
        {
            aviableStates[verb] = VerbToState(verb);
        }
    }

    private void Awake()
    {
        character = GetComponent<Character>();
        character.Setup();

        inputManager = GetComponent<InputManager>();
        inputManager.CreateInputMap();

        CreateStates();
        SetInitialSuperState();
    }

    private void FixedUpdate()
    {
        currentSuperState?.ConstantBehaviour();
        currentSuperState?.CheckTransition();

        currentState?.ConstantBehaviour();
        currentState?.CheckTransition();
    }
}
