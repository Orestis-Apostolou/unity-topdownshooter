using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

public class RLAgent : Agent
{
    [Header("Observations")]
    public GameObject enemy;
    private Rigidbody2D rb;

    private HealthSystem healthSys;
    private HeatSystem heatSys;
    private FiringSystem firingSys;

    [Header("Agent Properties")]
    public float movespeed = 6.0f;
    public float rotspeed = 130.0f;

    // protected override void Awake()
    // {

    //     base.Awake();
    // }

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSys = GetComponent<HealthSystem>();
        heatSys = GetComponent<HeatSystem>();
        firingSys = GetComponent<FiringSystem>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)transform.position);         // 2
        sensor.AddObservation((Vector2)enemy.transform.position);   // 2
        sensor.AddObservation((Vector2) transform.up);              // 2
        sensor.AddObservation(healthSys.HealthPercent());           // 1
        sensor.AddObservation(heatSys.HeatPercent());               // 1
        sensor.AddObservation(heatSys.IsOverheated);                // 1
    }

    // Actions in code as specified from the Inspector
    public override void OnActionReceived(ActionBuffers actions)
    {
        // CONTINUOUS ACTIONS

        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        float rotInput = actions.ContinuousActions[2];

        float effMovespeed = movespeed * (heatSys.IsOverheated ? heatSys.overheatSlow : 1f);

        Vector2 movement = new Vector2(moveX, moveY);
        rb.MovePosition(rb.position + movement * effMovespeed * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation + rotspeed * Time.fixedDeltaTime * -rotInput);

        // DISCRETE ACTIONS

        int shootAction = actions.DiscreteActions[0]; // 0 = don't shoot, 1 = shoot
        if (shootAction == 1 && firingSys.CanFire() && heatSys.CanFire())
        {
            firingSys.Fire();
            heatSys.Fire();
        }

    }

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     // Heuristic can help test the agent with manual input
    // }
}
