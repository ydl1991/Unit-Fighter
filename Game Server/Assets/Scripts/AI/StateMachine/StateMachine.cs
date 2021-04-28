using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public State currentState { get; private set; }

    public StateMachine()
    {
        currentState = null;
    }

    // Update is called once per frame
    public void UpdateStateMachine()
    {
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }

    public void SetCurrentState(State newState)
    {
        if (currentState != null)
        {
            currentState.ExitState();
        }

        currentState = newState;

        if (currentState != null)
        {
            currentState.EnterState();
        }
    }
}
