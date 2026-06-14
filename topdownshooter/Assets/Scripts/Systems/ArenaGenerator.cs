using System.Collections.Generic;
using UnityEngine;

public class ArenaGenerator : MonoBehaviour
{
    public static ArenaGenerator Instance { get; private set; }
    public float wallThickness = 1f;
    private Vector2 arenaSize = Vector2.zero;
    // public GameObject wallPrefab;
    private Vector2 camCenter;
    
    [Header("Layout Parameters")]
    public float layoutBlockSize = 5;

    [System.Serializable]
    public class BlockLayouts
    {
        public List<GameObject> possibleLayouts;
    }

    public BlockLayouts[] blocks = new BlockLayouts[9]; // row-major, index = (x+1) + (y+1)*3
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

        CreateWall("WallTop",    camCenter + new Vector2(0,  arenaSize.y / 2 + wallThickness / 2), new Vector2(arenaSize.x + 2 * wallThickness, wallThickness));
        CreateWall("WallBottom", camCenter + new Vector2(0, -arenaSize.y / 2 - wallThickness / 2), new Vector2(arenaSize.x + 2 * wallThickness, wallThickness));
        CreateWall("WallLeft",   camCenter + new Vector2(-arenaSize.x / 2 - wallThickness / 2, 0), new Vector2(wallThickness, arenaSize.y + 2 * wallThickness));
        CreateWall("WallRight",  camCenter + new Vector2( arenaSize.x / 2 + wallThickness / 2, 0), new Vector2(wallThickness, arenaSize.y + 2 * wallThickness));

        GenerateLayout();
    }

    public void GenerateLayout()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int index = (x + 1) + (y + 1) * 3;
                Vector2 blockCenter = camCenter + new Vector2(x * layoutBlockSize, y * layoutBlockSize);

                Debug.Log($"Block {index} center: {blockCenter}");

                List<GameObject> options = blocks[index].possibleLayouts;
                if (options == null || options.Count == 0)
                {
                    Debug.LogWarning($"[ProcGenerator] Block {index} has no layouts assigned, skipping.");
                    continue;
                }

                GameObject chosen = options[Random.Range(0, options.Count)];
                GameObject instance = Instantiate(chosen, blockCenter, Quaternion.identity);
                instance.transform.parent = layoutsParent;
            }
        }
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

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Vector2 camCenter = Camera.main.transform.position;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int index = (x + 1) + (y + 1) * 3;
                Vector2 blockCenter = camCenter + new Vector2(x * layoutBlockSize, y * layoutBlockSize);

                // Draw block boundary
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(blockCenter, Vector2.one * layoutBlockSize);

                // Draw index label at center
                UnityEditor.Handles.Label(blockCenter, $"Block {index}\n({x},{y})");
            }
        }
    }
    #endif
}
