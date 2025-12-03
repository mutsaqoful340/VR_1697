using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State currentState;
    public enum State
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Dead
    }
    // Keep track of previous state so we can detect transitions
    private State previousState;

    private void Awake()
    {
        // Ensure we have a default state
        previousState = currentState;
    }

    private void Update()
    {
        // If state changed, call OnStateEnter once
        if (previousState != currentState)
        {
            OnStateEnter(currentState);
            previousState = currentState;
        }

        // Per-frame handling for the current state
        HandleState();
    }

    // Public API to change state from other scripts
    public void SetState(State newState)
    {
        currentState = newState;
    }

    private void OnStateEnter(State s)
    {
        Debug.Log($"Entered state: {s}");
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case State.Idle:
                Debug.Log("Player is Idle");
                break;
            case State.Walking:
                Debug.Log("Player is Walking");
                break;
            case State.Running:
                Debug.Log("Player is Running");
                break;
            case State.Jumping:
                Debug.Log("Player is Jumping");
                break;
            case State.Dead:
                Debug.Log("Player is Dead");
                break;
            default:
                Debug.Log("Unknown State");
                break;
        }
    }
}
