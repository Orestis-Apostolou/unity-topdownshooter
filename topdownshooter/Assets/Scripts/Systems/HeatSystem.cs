using System;
using UnityEngine;

public class HeatSystem : MonoBehaviour
{
    [Header("Heat Settings")]
    public float maxHeat = 50f;
    public float recoveryRate = 15f;
    public float heatPerShot = 10f;

    private float heat = 0f;
    private float overheatTimer = 0f;
    private bool isOverheated = false;
    private HealthSystem healthSystem;

    [Header("Overheat Settings")]
    public float dangerThresh = 0.8f;       // Threshold at which you start taking self damage when shooting
    public float dangerDmgPerShot = 5f;     // Points of self damage when firing while above 'dangerThresh'% heat
    public float overheatSlowDur = 3f;      // Seconds of slow when hitting 100% on heat
    public float overheatSlow = 0.35f;      // Slow factor
    
    public bool IsOverheated => isOverheated;

    public bool CanFire()
    {
        return !isOverheated;
    }

    public bool CanFireWithoutOH()
    {
        if (isOverheated)
            return false;

        return heat + heatPerShot <= maxHeat;
    }

    public bool CanFireSafe()
    {
        if (HeatPercent() <= dangerThresh)
            return true;

        return false;
    }

    public void Fire()
    {
        if (HeatPercent() >= dangerThresh)
            healthSystem.TakeDamage(dangerDmgPerShot);

        heat += heatPerShot;

        if (heat >= maxHeat)
        {
            isOverheated = true;
            overheatTimer = overheatSlowDur;
        }
            
    }
    public float HeatPercent()
    {
        return heat / maxHeat;
    }

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    void Update()
    {
        if (isOverheated)
        {
            overheatTimer -= Time.deltaTime;
            if (overheatTimer <= 0f)
                isOverheated = false;
        }

        if (heat >= 0f)
            heat = Mathf.Max(0f, heat - recoveryRate * Time.deltaTime);
    }
}
