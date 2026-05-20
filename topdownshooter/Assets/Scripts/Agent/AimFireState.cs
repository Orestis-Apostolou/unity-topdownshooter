using UnityEngine;

public class AimFireState : State
{
    public AimFireState(AgentController agent) : base(agent) { }

    public override void FixedUpdate()
    {
        // Aim and fire if able
        if (!IsAimed())
        {
            Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
            float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
            float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
            agent.rb.MoveRotation(newRotation);
        }
        else if (agent.firingSystem.CanFire() && agent.heatSystem.CanFire())
        {
            agent.heatSystem.Fire();
            agent.firingSystem.Fire();
        }

        // State Transitions
        if (agent.heatSystem.HeatPercent() > 0.8f)
        {
            agent.stateMachine.ChangeState(new OverheatedState(agent));
        } else if (!agent.firingSystem.IsInRange(agent.player.transform.position, 0.8f))
        {
            agent.stateMachine.ChangeState(new ApproachState(agent));
        }

    }

    // Check if agent is looking within {aimAngle} degrees of target
    private bool IsAimed()
    {
        Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        return Mathf.Abs(Mathf.DeltaAngle(agent.rb.rotation, targetAngle)) < agent.aimAngle;
    }
}
