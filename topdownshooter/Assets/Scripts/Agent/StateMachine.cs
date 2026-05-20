using UnityEngine;

public class StateMachine
{
    private State curState;

    public void ChangeState(State newState)
    {
        curState?.Exit();
        curState = newState;
        curState.Enter();
    }

    public void Update() => curState?.Update();
    public void FixedUpdate() => curState?.FixedUpdate();

}
