using UnityEngine;

public class BaselineOverheatedState : State
{
    public BaselineOverheatedState(AgentController agent) : base(agent) { }

    public override void FixedUpdate()
    {
        Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
        agent.rb.MoveRotation(newRotation);

        if (!agent.heatSystem.IsOverheated && agent.heatSystem.HeatPercent() <= 0.1f)
            agent.stateMachine.ChangeState(new BaselineOutOfRangeState(agent));
    }
}
