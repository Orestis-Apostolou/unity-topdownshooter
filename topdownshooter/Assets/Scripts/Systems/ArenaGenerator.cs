using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.VisualScripting;
using UnityEngine;

public class ArenaGenerator : MonoBehaviour
{
    public static ArenaGenerator Instance { get; private set; }
    public float wallThickness = 1f;
    private Vector2 arenaSize = Vector2.zero;
    // public GameObject wallPrefab;
    private Vector2 camCenter;
    
    [Header("Cluster Parameters")]
    //public float layoutBlockSize = 5;

    public List<GameObject> clusterPrefabs;
    public List<float> clusterRadii;

    public int clusterCount = 10;
    public float padding = 2f;
    public int maxAttempts = 50;

    private Transform layoutsParent;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Camera cam = Camera.main;
        camCenter = cam.transform.position;

        layoutsParent = new GameObject("GeneratedLayouts").transform;
        layoutsParent.parent = transform;

        float height = cam.orthographicSize * 2f;
        float width = height * (16f / 9f);
        arenaSize = new Vector2(width, height);

        // Create unbreakable arena outer bounds
        CreateWall("WallTop",    camCenter + new Vector2(0,  arenaSize.y / 2 + wallThickness / 2), new Vector2(arenaSize.x + 2 * wallThickness, wallThickness));
        CreateWall("WallBottom", camCenter + new Vector2(0, -arenaSize.y / 2 - wallThickness / 2), new Vector2(arenaSize.x + 2 * wallThickness, wallThickness));
        CreateWall("WallLeft",   camCenter + new Vector2(-arenaSize.x / 2 - wallThickness / 2, 0), new Vector2(wallThickness, arenaSize.y + 2 * wallThickness));
        CreateWall("WallRight",  camCenter + new Vector2( arenaSize.x / 2 + wallThickness / 2, 0), new Vector2(wallThickness, arenaSize.y + 2 * wallThickness));

        // Populate arena with clusters
        StartCoroutine(GenerateLayout());
    }

    private List<(Vector2 pos, float r)> placedPositions = new List<(Vector2, float)>();

    public IEnumerator GenerateLayout()
    {
        placedPositions.Clear();

        placedPositions.Add(( (Vector2)GameManager.Instance.playerSpawn, padding ));
        placedPositions.Add(( (Vector2)GameManager.Instance.enemySpawn, padding ));

        int placed = 0;
        int attempts = 0;

        // Randomize the number of attempts (more diversly populated arenas)
        //maxAttempts = Random.Range(10, 301);

        while (placed < clusterCount && attempts < maxAttempts)
        {
            // The +/- x represents padding from the arena walls
            Vector2 candidate = new Vector2(
                Random.Range(-arenaSize.x / 2 + 1.5f, arenaSize.x / 2 - 1.5f),
                Random.Range(-arenaSize.y / 2 + 1.5f, arenaSize.y / 2 - 1.5f)
            );

            // Pick random cluster for candidate
            int index = Random.Range(0, clusterPrefabs.Count);
            float radius = clusterRadii[index];

            if (index >= clusterRadii.Count)
                Debug.LogError("Dimension mismatch between clusterPrefabs and clusterRadii");

            if (IsFarEnough(candidate, radius))
            {
                // If the random cluster is far enough from neighbors then place
                GameObject cluster = Instantiate(clusterPrefabs[index], candidate, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
                cluster.transform.parent = layoutsParent.transform;
                placedPositions.Add((candidate, radius));

                placed++;
            }

            attempts++;
        }

        // Wait one frame for Unity to register new objects
        yield return StartCoroutine(NavMeshUpdater.Instance.UpdateAndWait());
    }

    bool IsFarEnough(Vector2 candPos, float candRadius)
    {
        foreach (var (pos, r) in placedPositions)
        {
            if (Vector2.Distance(candPos, pos) < candRadius + r)
                return false;
        }
        return true;
    }

    public void DestroyLayout()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in layoutsParent)
            children.Add(child);
        
        foreach (Transform child in children)
            DestroyImmediate(child.gameObject);
    }

    private void CreateWall(string name, Vector2 position, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.parent = transform;
        wall.transform.position = position;

        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = size;

        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite();
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
    }

    private Sprite CreateRectSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
    }

    //#if UNITY_EDITOR
    //private void OnDrawGizmos()
    //{
    //    // Vector2 camCenter = Camera.main.transform.position;

    //    for (int x = -1; x <= 1; x++)
    //    {
    //        for (int y = -1; y <= 1; y++)
    //        {
    //            int index = (x + 1) + (y + 1) * 3;
    //            Vector2 blockCenter = camCenter + new Vector2(x * layoutBlockSize, y * layoutBlockSize);

    //            // Draw block boundary
    //            Gizmos.color = Color.yellow;
    //            Gizmos.DrawWireCube(blockCenter, Vector2.one * layoutBlockSize);

    //            // Draw index label at center
    //            UnityEditor.Handles.Label(blockCenter, $"Block {index}\n({x},{y})");
    //        }
    //    }
    //}
    //#endif
}
