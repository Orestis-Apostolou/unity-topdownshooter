using UnityEngine;

public class OverheatedState : State
{
    private float strafeDirection = 1f;
    private float strafeTimer = 0f;
    private float strafeDuration;

    public OverheatedState(AgentController agent) : base(agent) { }

    public override void Enter()
    {
        ResetStrafe();
    }
    public override void FixedUpdate()
    {
        // Strafe
        Vector2 right = agent.transform.right;
        agent.rb.MovePosition(agent.rb.position + right * strafeDirection * agent.movespeed * Time.fixedDeltaTime);

        // Count down and switch direction
        strafeTimer += Time.fixedDeltaTime;
        if (strafeTimer >= strafeDuration)
        {
            strafeDirection *= -1f;
            ResetStrafe();
        }

        // Transition
        if (agent.heatSystem.HeatPercent() <= 0.3f)
            agent.stateMachine.ChangeState(new ApproachState(agent));
    }

    private void ResetStrafe()
    {
        strafeTimer = 0f;
        strafeDuration = Random.Range(0.5f, 1.5f);
    }

}
