using UnityEditor.Rendering;
using UnityEngine;

public class ObstacleHealth : HealthSystem
{
    void OnDestroy()
    {
        // Trigger a navmesh update only if the round is ongoing
        if (!GameManager.Instance.roundOver)
            NavMeshUpdater.Instance.UpdateMesh();
    }
}
