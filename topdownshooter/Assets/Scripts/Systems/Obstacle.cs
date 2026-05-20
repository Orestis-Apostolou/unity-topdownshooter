using UnityEngine;

public class Obstacle : HealthSystem
{
    public void OnDestroy()
    {
        NavMeshUpdater.instance.UpdateMesh();
    }
}
