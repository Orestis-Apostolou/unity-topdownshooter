using UnityEditor.Rendering;
using UnityEngine;

public class Obstacle : HealthSystem
{
    void OnDisable()
    {
        NavMeshUpdater.Instance.UpdateMesh();
    }

    void OnEnable()
    {
        NavMeshUpdater.Instance.UpdateMesh();  
    }

    protected override void OnDeath()
    {
        gameObject.SetActive(false);
    }
}
