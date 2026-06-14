using UnityEngine;
using System.Collections;
using NavMeshPlus.Components;

public class NavMeshUpdater : MonoBehaviour
{
    public static NavMeshUpdater Instance;
    private NavMeshSurface surface;

    void Awake()
    {
        Instance = this;
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

    public IEnumerator UpdateAndWait()
    {
        var op = surface.UpdateNavMesh(surface.navMeshData);
        yield return op; // waits until navmesh is fully built
    }
}
