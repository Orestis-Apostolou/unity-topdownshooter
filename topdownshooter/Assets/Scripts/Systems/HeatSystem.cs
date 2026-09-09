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
    //private HealthSystem healthSystem;

    [Header("Overheat Settings")]
    public float overheatSlowDur = 3f;      // Seconds of slow when hitting 100% on heat
    public float overheatSlow = 1f;         // Slow factor (inactive)
    public float overheatThresh = 0.9f;
    
    public bool IsOverheated => isOverheated;

    public bool CanFire()
    {
        return !isOverheated;
    }

    public bool CanFireSafe()
    {
        if (isOverheated)
            return false;

        return heat + heatPerShot <= maxHeat;
    }

    public void Fire()
    {
        heat += heatPerShot;

        if (heat >= maxHeat * overheatThresh)
        {
            isOverheated = true;
            overheatTimer = overheatSlowDur;
        }
            
    }
    public float HeatPercent()
    {
        return heat / maxHeat;
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

    public void ResetHeat()
    {
        isOverheated = false;
        overheatTimer = 0f;
        heat = 0f;
    }
}
