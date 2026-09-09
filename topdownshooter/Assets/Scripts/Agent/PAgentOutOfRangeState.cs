using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
public class PAgentOutOfRangeState : State
{
    private float checkTimer = 0f;
    private float checkInterval = PAgentDestroyCoverState.checkInterval;
    private float coverHpThreshold = PAgentDestroyCoverState.hpThreshold;

    public PAgentOutOfRangeState(AgentController agent) : base(agent)
    {
        Debug.Log("Out of Range");
        agent.navAgent.updateRotation = false;
        agent.navAgent.updateUpAxis = false;
    }

    public override void FixedUpdate()
    {
        agent.navAgent.speed = agent.EffectiveMoveSpeed;
        agent.navAgent.SetDestination(agent.player.transform.position);

        NavMeshHit hit;
        // Guard if enemy is slightly off of the navmesh
        if (NavMesh.SamplePosition(agent.player.transform.position, out hit, 3.0f, NavMesh.AllAreas))
        {
            agent.navAgent.SetDestination(hit.position);
        }

        RotateTowardPlayer();

        if (HasLOS() && agent.firingSystem.IsInRange(agent.player.transform.position, 0.75f) && IsAimed())
        {
            // Enemy in range and LOS?
            agent.stateMachine.ChangeState(new PAgentInRangeState(agent));
            return;
        }

        checkTimer -= Time.fixedDeltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = checkInterval;

            if (agent.firingSystem.IsInRange(agent.player.transform.position, 0.75f))
            {
                var result = PAgentController.CoverChecker.CheckCover(
                    agent.transform.position,
                    agent.player.transform.position
                );

                if (!result.hasLineOfSight && result.totalHP < coverHpThreshold)
                {
                    // Enemy in range and behind weak cover?
                    agent.stateMachine.ChangeState(new PAgentDestroyCoverState(agent));
                    return;
                }
            }
        }

        // If the enemy is behind strong cover, or out of range and LOS
        // path to them via navmesh
    }

    private void RotateTowardPlayer()
    {
        Vector2 direction = (GetPredictedPosition() - (Vector2) agent.transform.position).normalized;
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

        RaycastHit2D[] hits = Physics2D.RaycastAll(agent.transform.position, direction, distance);

        RaycastHit2D closest = default;
        float closestDist = float.MaxValue;
        bool found = false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == agent.gameObject) continue;
            if (hit.collider.CompareTag("Bullet")) continue;

            if (hit.distance < closestDist)
            {
                closestDist = hit.distance;
                closest = hit;
                found = true;
            }
        }

        return found && closest.collider.gameObject == agent.player;
    }
}