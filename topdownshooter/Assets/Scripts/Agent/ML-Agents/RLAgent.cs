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

    private HealthSystem enemyHealthSys;
    private HeatSystem enemyHeatSys;


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

        enemyHealthSys = enemy.GetComponent<HealthSystem>();
        enemyHeatSys = enemy.GetComponent<HeatSystem>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 toEnemy = (Vector2)(enemy.transform.position - transform.position);
        float angleToEnemy = Vector2.SignedAngle(transform.up, toEnemy.normalized);

        sensor.AddObservation((Vector2)toEnemy);                    // 2
        sensor.AddObservation(angleToEnemy / 180f);                 // 1
        sensor.AddObservation((Vector2)transform.up);               // 2
        sensor.AddObservation((Vector2)enemy.transform.up);         // 2
        sensor.AddObservation(healthSys.HealthPercent());           // 1
        sensor.AddObservation(heatSys.HeatPercent());               // 1
        sensor.AddObservation(heatSys.IsOverheated);                // 1
        sensor.AddObservation(enemyHealthSys.HealthPercent());      // 1
        sensor.AddObservation(firingSys.CanFire());                 // 1
        sensor.AddObservation(rb.linearVelocity);                   // 2
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

        //// Heat penalty proportional to current heat
        //AddReward(-0.001f * heatSys.HeatPercent());

        //Vector2 toPlayer = (transform.position - enemy.transform.position);
        //float dot = Vector2.Dot((Vector2)enemy.transform.up, toPlayer.normalized);
        //if (dot > 0.9f) // enemy is roughly aimed at you
        //{
        //    RaycastHit2D hit = Physics2D.Raycast(
        //        enemy.transform.position,
        //        toPlayer.normalized,
        //        toPlayer.magnitude,
        //        LayerMask.GetMask("Default") // replace with whatever your wall layer is
        //    );
        //    if (hit.collider != null && hit.collider.gameObject == gameObject)
        //        AddReward(-0.002f); // in LOS and aimed at
        //}

        //Vector2 toEnemy = (enemy.transform.position - transform.position).normalized;
        //float facing = Vector2.Dot((Vector2)transform.up, toEnemy);
        //AddReward(facing * 0.004f);
    }

    //private void Update()
    //{
    //    Debug.Log("Observation Count: " + GetObservations().Count);
    //}

    private float timerMax = 1f;
    private float timerCur;

    private void FixedUpdate()
    {
        timerCur += Time.fixedDeltaTime;
        if (timerCur >= timerMax)
        {
            AddReward(-0.001f);
            timerCur = 0f;
        }
    }
}
