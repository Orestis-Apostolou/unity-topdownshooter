using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.U2D;

public class PAgentOverheatedState : State
{
    private float angleRange = 30.0f;
    private float delay = 0.0f;
    private float delayInterval = 0.3f;

    public PAgentOverheatedState(AgentController agent) : base(agent) { }

    public override void FixedUpdate()
    {
        delay -= Time.fixedDeltaTime;
        if (delay <= 0.0f)
        {
            // Direction opposite to the player
            Vector2 fleeDirection = ((Vector2)agent.transform.position - (Vector2)agent.player.transform.position).normalized;
            float fleeAngle = Mathf.Atan2(fleeDirection.y, fleeDirection.x) * Mathf.Rad2Deg;

            float randomAngle = (Random.Range(angleRange, 180 - angleRange) + fleeAngle) * Mathf.Deg2Rad;
            fleeDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

            Vector2 fleePoint = (Vector2)agent.transform.position + fleeDirection * 8f;
            agent.navAgent.speed = agent.EffectiveMoveSpeed;
            agent.navAgent.SetDestination(fleePoint);

            delay = delayInterval;
        }

        Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
        agent.rb.MoveRotation(newRotation);

        if (!agent.heatSystem.IsOverheated && agent.heatSystem.HeatPercent() <= 0.1f)
            agent.stateMachine.ChangeState(new PAgentOutOfRangeState(agent));
    }

    //private void RotateTowardPlayer()
    //{
    //    Vector2 direction = (agent.player.transform.position - agent.transform.position).normalized;
    //    float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
    //    float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
    //    agent.rb.MoveRotation(newRotation);
    //}
}
