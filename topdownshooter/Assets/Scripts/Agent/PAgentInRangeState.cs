using UnityEngine;

// NOTE: Agent might struggle with close range if the target is moving a lot
public class PAgentInRangeState : State
{
    public PAgentInRangeState(AgentController agent) : base(agent) { }

    public override void Enter()
    {
        Debug.Log("Entered InRangeState");
        //agent.navAgent.isStopped = true;
    }

    public override void Exit()
    {
        agent.navAgent.isStopped = false;
    }

    public override void FixedUpdate()
    {
        RotateTowardPlayer();

        if (HasLOS() && IsAimed() && agent.firingSystem.CanFire() && agent.heatSystem.CanFireSafe())
        {
            agent.firingSystem.Fire();
            agent.heatSystem.Fire();
        }

        if (agent.heatSystem.HeatPercent() >= 0.85f)
        {
            // Overheated?
            agent.stateMachine.ChangeState(new PAgentOverheatedState(agent));
            return;
        }
        else if (!HasLOS() || !agent.firingSystem.IsInRange(agent.player.transform.position, 0.75f))
        {
            // Enemy out of range?
            agent.stateMachine.ChangeState(new PAgentOutOfRangeState(agent));
            return;
        }
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
    private bool HasLOS()
    {
        Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
        float distance = Vector2.Distance(agent.transform.position, agent.player.transform.position);

        // Cast a ray to see if the player is behind a wall (loop to ignore accidental self collision)
        RaycastHit2D[] hits = Physics2D.RaycastAll(agent.transform.position, direction, distance);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == agent.gameObject) continue;
            return hit.collider.gameObject == agent.player;
        }
        return false;
    }
}