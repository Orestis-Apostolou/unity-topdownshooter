using UnityEngine;

public class PAgentDestroyCoverState : State
{
    public const float hpThreshold = 100f;
    private const float maxHeatPercent = 0.60f;
    public const float checkInterval = 0.1f;

    private float checkTimer = 0f;

    public PAgentDestroyCoverState(AgentController agent) : base(agent) { }

    public override void Enter()
    {
        //Debug.Log("Entered DestroyCoverState");
    }

    public override void FixedUpdate()
    {
        RotateTowardPlayer();

        checkTimer -= Time.fixedDeltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = checkInterval;

            var result = PAgentController.CoverChecker.CheckCover(
                agent.transform.position,
                agent.player.transform.position
            );

            // Line of sight is clear
            if (result.hasLineOfSight)
            {
                agent.stateMachine.ChangeState(new PAgentInRangeState(agent));
                return;
            }

            // Cover too strong or agent out of range, reposition instead
            if (result.totalHP > hpThreshold || 
                !agent.firingSystem.IsInRange(agent.player.transform.position, 0.75f))
            {
                agent.stateMachine.ChangeState(new PAgentOutOfRangeState(agent));
                return;
            }
        }

        // Fire at cover if heat allows and it's worth breaking
        if (!agent.heatSystem.IsOverheated &&
            agent.heatSystem.HeatPercent() <= maxHeatPercent &&
            agent.firingSystem.CanFire())
        {
            agent.firingSystem.Fire();
            agent.heatSystem.Fire();
        }
    }

    private void RotateTowardPlayer()
    {
        Vector2 direction = ((Vector2)agent.player.transform.position - (Vector2)agent.transform.position).normalized;
        float targetAngle = Vector2.SignedAngle(Vector2.up, direction);
        float newRotation = Mathf.MoveTowardsAngle(agent.rb.rotation, targetAngle, agent.rotspeed * Time.fixedDeltaTime);
        agent.rb.MoveRotation(newRotation);
    }
}
