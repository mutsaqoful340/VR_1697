using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine_Test : MonoBehaviour
{
    public StateMachine stateMachine;

    void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
    }

    public void Dead()
    {
        stateMachine.currentState = StateMachine.State.Dead;
    }

    // Example helper methods to change state from other scripts or UI buttons
    public void SetIdle()
    {
        if (stateMachine != null) stateMachine.SetState(StateMachine.State.Idle);
    }

    public void SetWalking()
    {
        if (stateMachine != null) stateMachine.SetState(StateMachine.State.Walking);
    }

    public void SetRunning()
    {
        if (stateMachine != null) stateMachine.SetState(StateMachine.State.Running);
    }

    public void SetJumping()
    {
        if (stateMachine != null) stateMachine.SetState(StateMachine.State.Jumping);
    }

    // Simple demo: press keys 1-5 to change states at runtime
    private void Update()
    {
        if (stateMachine == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetIdle();
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetWalking();
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetRunning();
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetJumping();
        if (Input.GetKeyDown(KeyCode.Alpha5)) Dead();
    }
}
