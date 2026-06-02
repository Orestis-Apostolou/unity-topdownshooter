using System.Collections.Generic;
using UnityEngine;

public class ProcGenerator : MonoBehaviour
{
    public float wallThickness = 1f;
    private Vector2 arenaSize = Vector2.zero;
    public GameObject wallPrefab;

    public int clusterCount = 6;
    public int minClusterSize = 3;
    public int maxClusterSize = 6;
    public float centerClearRadius = 3f;

    private float tileSize;
    private int gridWidth;
    private int gridHeight;
    private bool[,] occupied;

    void Start()
    {
        Camera cam = Camera.main;
        float height = cam.orthographicSize * 2f;
        float width = height * (16 / 9f);
        arenaSize = new Vector2(width, height);

        Debug.Log(arenaSize);

        CreateWall("WallTop", new Vector2(0, arenaSize.y / 2 + wallThickness / 2), new Vector2(arenaSize.x + 2 * wallThickness, wallThickness));
        CreateWall("WallBottom", new Vector2(0, -arenaSize.y / 2 - wallThickness / 2), new Vector2(arenaSize.x + 2 * wallThickness, wallThickness));
        CreateWall("WallLeft", new Vector2(-arenaSize.x / 2 - wallThickness / 2, 0), new Vector2(wallThickness, arenaSize.y + 2 * wallThickness));
        CreateWall("WallRight", new Vector2(arenaSize.x / 2 + wallThickness / 2, 0), new Vector2(wallThickness, arenaSize.y + 2 * wallThickness));

        GenerateClusters();
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
    private void GenerateClusters()
    {
        tileSize = wallPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        gridWidth = Mathf.FloorToInt(arenaSize.x / tileSize);
        gridHeight = Mathf.FloorToInt(arenaSize.y / tileSize);
        occupied = new bool[gridWidth, gridHeight];

        for (int i = 0; i < clusterCount; i++)
        {
            Vector2Int seed = GetRandomSeed();
            GrowCluster(seed);
        }
    }

    private Vector2Int GetRandomSeed()
    {
        Vector2Int seed;
        int attempts = 0;
        do
        {
            seed = new Vector2Int(
                Random.Range(2, gridWidth - 2),
                Random.Range(2, gridHeight - 2)
            );
            attempts++;
        }
        while (IsTooCloseToCenter(seed) && attempts < 100);
        return seed;
    }

    private bool IsTooCloseToCenter(Vector2Int gridPos)
    {
        Vector2 worldPos = GridToWorld(gridPos.x, gridPos.y);
        return worldPos.magnitude < centerClearRadius;
    }

    private void GrowCluster(Vector2Int seed)
    {
        int clusterSize = Random.Range(minClusterSize, maxClusterSize);
        List<Vector2Int> cluster = new List<Vector2Int> { seed };
        occupied[seed.x, seed.y] = true;

        for (int i = 1; i < clusterSize; i++)
        {
            Vector2Int current = cluster[Random.Range(0, cluster.Count)];
            Vector2Int[] neighbours = {
            current + Vector2Int.up,
            current + Vector2Int.down,
            current + Vector2Int.left,
            current + Vector2Int.right
        };

            foreach (Vector2Int neighbour in neighbours)
            {
                if (IsValidGridPos(neighbour) && !occupied[neighbour.x, neighbour.y] && !IsTooCloseToCenter(neighbour))
                {
                    cluster.Add(neighbour);
                    occupied[neighbour.x, neighbour.y] = true;
                    break;
                }
            }
        }

        foreach (Vector2Int pos in cluster)
            SpawnWall(pos);
    }

    private void SpawnWall(Vector2Int gridPos)
    {
        Vector2 worldPos = GridToWorld(gridPos.x, gridPos.y);
        GameObject wall = Instantiate(wallPrefab, worldPos, Quaternion.identity);
        wall.transform.parent = transform;
    }

    private Vector2 GridToWorld(int x, int y)
    {
        return new Vector2(
            -arenaSize.x / 2 + x * tileSize + tileSize / 2,
            -arenaSize.y / 2 + y * tileSize + tileSize / 2
        );
    }

    private bool IsValidGridPos(Vector2Int pos)
    {
        return pos.x >= 1 && pos.x < gridWidth - 1 &&
               pos.y >= 1 && pos.y < gridHeight - 1;
    }
}
