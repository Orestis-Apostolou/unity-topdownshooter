using UnityEngine;

public class BaselineAgentController : AgentController
{
    protected override void Start()
    {
        stateMachine.ChangeState(new BaselineOutOfRangeState(this));
    }
}
