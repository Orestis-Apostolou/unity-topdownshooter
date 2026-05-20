using UnityEngine;
public class ApproachState : State
{
    public ApproachState(AgentController agent) : base(agent) { }
    public override void FixedUpdate()
    {
        Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        bool isAimed = Mathf.Abs(Mathf.DeltaAngle(agent.rb.rotation, targetAngle)) < agent.aimAngle;

        // Move toward player
        agent.rb.MovePosition(agent.rb.position + direction * agent.movespeed * Time.fixedDeltaTime);
        // Rotate toward player
        float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
        agent.rb.MoveRotation(newRotation);
        // Transition
        if (agent.firingSystem.IsInRange(agent.player.transform.position, 0.8f))
            agent.stateMachine.ChangeState(new AimFireState(agent));
        else if (agent.heatSystem.HeatPercent() > 0.8f)
            agent.stateMachine.ChangeState(new OverheatedState(agent));
    }
}