using UnityEngine;

public class BaselineInRangeState : State
{
    public BaselineInRangeState(AgentController agent) : base(agent) { }

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

        if (IsAimed() && HasLOS() && agent.firingSystem.CanFire() && agent.heatSystem.CanFireSafe())
        {
            agent.firingSystem.Fire();
            agent.heatSystem.Fire();
        }

        if (agent.heatSystem.HeatPercent() >= 0.85f)
            agent.stateMachine.ChangeState(new BaselineOverheatedState(agent));
        else if (!agent.firingSystem.IsInRange(agent.player.transform.position, 0.75f) || !HasLOS())
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