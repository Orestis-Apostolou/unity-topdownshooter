using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
public class PAgentOutOfRangeState : State
{
    public PAgentOutOfRangeState(AgentController agent) : base(agent)
    {
        agent.navAgent.updateRotation = false;
        agent.navAgent.updateUpAxis = false;
    }

    public override void FixedUpdate()
    {
        agent.navAgent.speed = agent.EffectiveMoveSpeed;
        agent.navAgent.SetDestination(agent.player.transform.position);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(agent.player.transform.position, out hit, 3.0f, NavMesh.AllAreas))
        {
            agent.navAgent.SetDestination(hit.position);
        }

        RotateTowardPlayer();

        if (agent.firingSystem.IsInRange(agent.player.transform.position) && HasLOS() && IsAimed())
            agent.stateMachine.ChangeState(new PAgentInRangeState(agent));
    }

    private void RotateTowardPlayer()
    {
        Vector2 direction = (GetPredictedPosition() - (Vector2) agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
        agent.rb.MoveRotation(newRotation);
    }

    private bool HasLOS()
    {
        Vector2 direction = (GetPredictedPosition() - (Vector2) agent.transform.position).normalized;
        float distance = Vector2.Distance(agent.transform.position, agent.player.transform.position);

        RaycastHit2D[] hits = Physics2D.RaycastAll(agent.transform.position, direction, distance);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == agent.gameObject) continue;
            if (hit.collider.gameObject == agent.player) continue;
            return false; // Another object besides the player / agent is in the way
        }
        return true; // Clear LOS to the predicted player position
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