using UnityEditor.Rendering;
using UnityEngine;

public class Obstacle : HealthSystem
{
    void OnDisable()
    {
        NavMeshUpdater.instance.UpdateMesh();
    }

    void OnEnable()
    {
        NavMeshUpdater.instance.UpdateMesh();  
    }

    protected override void OnDeath()
    {
        gameObject.SetActive(false);
    }
}
