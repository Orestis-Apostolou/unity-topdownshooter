using UnityEngine;

public abstract class State
{
    protected AgentController agent;
    
    public State(AgentController agent)
    {
        this.agent = agent;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }

    public virtual void Exit() { }
}
