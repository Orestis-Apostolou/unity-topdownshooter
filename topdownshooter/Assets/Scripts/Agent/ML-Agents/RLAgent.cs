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
    }

    // Actions in code as specified from the Inspector
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        float rotInput = actions.ContinuousActions[2];

        float effMovespeed = movespeed * (heatSys.IsOverheated ? heatSys.overheatSlow : 1f);

        Vector2 movement = new Vector2(moveX, moveY); //.normalized ?
        rb.MovePosition(rb.position + movement * effMovespeed * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation + rotspeed * Time.fixedDeltaTime * -rotInput);
    }

    public override void OnEpisodeBegin()
    {
        
    }

}
