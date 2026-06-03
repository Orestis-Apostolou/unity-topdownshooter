#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CenterPrefabContents : EditorWindow
{
    [MenuItem("Tools/Center Prefab Contents")]
    static void Run()
    {
        GameObject prefab = Selection.activeGameObject;
        if (prefab == null) return;

        // Find bounds center of all children
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        Vector3 offset = bounds.center - prefab.transform.position;

        // Shift all children by negative offset
        foreach (Transform child in prefab.transform)
            child.position -= offset;

        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log($"Centered {prefab.name}, offset was {offset}");
    }
}
#endif
