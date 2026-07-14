using UnityEditor.Rendering;
using UnityEngine;

// NOTE: Agent might struggle with close range if the target is moving a lot
public class PAgentInRangeState : State
{
    public PAgentInRangeState(AgentController agent) : base(agent) { }

    public override void Enter()
    {
        agent.navAgent.isStopped = true;
    }

    public override void Exit()
    {
        agent.navAgent.isStopped = false;
    }

    public override void FixedUpdate()
    {
        RotateTowardPlayer();

        if (IsAimed() && agent.firingSystem.CanFire() && agent.heatSystem.CanFireSafe())
        {
            agent.firingSystem.Fire();
            agent.heatSystem.Fire();
        }

        if (agent.heatSystem.HeatPercent() >= 0.85f)
            agent.stateMachine.ChangeState(new PAgentOverheatedState(agent));
        else if (!agent.firingSystem.IsInRange(agent.player.transform.position, 0.75f))
            agent.stateMachine.ChangeState(new PAgentOutOfRangeState(agent));
    }

    private void RotateTowardPlayer()
    {
        Vector2 direction = (GetPredictedPosition() - (Vector2)agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
        agent.rb.MoveRotation(newRotation);
    }

    private bool IsAimed()
    {
        Vector2 predicted = GetPredictedPosition();
        Vector2 direction = (predicted - (Vector2)agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);

        return Mathf.Abs(Mathf.DeltaAngle(agent.rb.rotation, targetAngle)) < agent.aimAngle;
    }

    private Vector2 GetPredictedPosition()
    {
        // Predict where the player will be if they keep moving with the same movespeed
        Vector2 playerPos = agent.player.transform.position;
        float distance = Vector2.Distance(agent.transform.position, playerPos);
        float timeToHit = distance / agent.firingSystem.getProjSpeed();

        return playerPos + agent.playerVelocity * timeToHit;
    }
}