using UnityEngine;

public class BaselineInRangeState : State
{
    public BaselineInRangeState(AgentController agent) : base(agent) { }

    public override void Enter()
    {
        agent.navAgent.enabled = false;
    }

    public override void Exit()
    {
        agent.navAgent.enabled = true;
    }

    public override void FixedUpdate()
    {
        RotateTowardPlayer();

        if (IsAimed() && agent.firingSystem.CanFire() && agent.heatSystem.CanFireSafe())
        {
            agent.firingSystem.Fire();
            agent.heatSystem.Fire();
        }

        if (agent.heatSystem.HeatPercent() >= agent.heatSystem.dangerThresh)
            agent.stateMachine.ChangeState(new BaselineOverheatedState(agent));
        else if (!agent.firingSystem.IsInRange(agent.player.transform.position, 0.75f))
            agent.stateMachine.ChangeState(new BaselineOutOfRangeState(agent));
    }

    private void RotateTowardPlayer()
    {
        Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
        agent.rb.MoveRotation(newRotation);
    }

    private bool IsAimed()
    {
        Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        return Mathf.Abs(Mathf.DeltaAngle(agent.rb.rotation, targetAngle)) < agent.aimAngle;
    }
}