using UnityEngine;

public class PAgentController : AgentController
{
    protected override void Start()
    {
        lastPlayerPos = player.transform.position;
        stateMachine.ChangeState(new PAgentOutOfRangeState(this));
    }
}
