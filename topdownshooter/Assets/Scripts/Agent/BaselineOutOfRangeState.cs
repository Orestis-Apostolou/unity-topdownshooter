using UnityEngine;
using UnityEngine.AI;

public class BaselineOutOfRangeState : State
{

    public BaselineOutOfRangeState(AgentController agent) : base(agent)
    {
        agent.navAgent.updateRotation = false;
        agent.navAgent.updateUpAxis = false;
    }

    public override void FixedUpdate()
    {
        agent.navAgent.speed = agent.EffectiveMoveSpeed;
        agent.navAgent.SetDestination(agent.player.transform.position);

        RotateTowardPlayer();

        if (agent.firingSystem.IsInRange(agent.player.transform.position) && HasLOS() && IsAimed())
            agent.stateMachine.ChangeState(new BaselineInRangeState(agent));
    }

    private void RotateTowardPlayer()
    {
        Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
        agent.rb.MoveRotation(newRotation);
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

    private bool IsAimed()
    {
        Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        return Mathf.Abs(Mathf.DeltaAngle(agent.rb.rotation, targetAngle)) < agent.aimAngle;
    }
}