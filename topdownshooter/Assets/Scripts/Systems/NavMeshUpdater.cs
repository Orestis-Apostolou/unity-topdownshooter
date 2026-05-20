using UnityEngine;

using NavMeshPlus.Components;

public class NavMeshUpdater : MonoBehaviour
{
    public static NavMeshUpdater instance;
    private NavMeshSurface surface;

    void Awake()
    {
        instance = this;
        surface = GetComponent<NavMeshSurface>();
    }

    void Start()
    {
        surface.BuildNavMeshAsync();
    }

    public void UpdateMesh()
    {
        if (surface != null)
            surface.UpdateNavMesh(surface.navMeshData);
    }
}
