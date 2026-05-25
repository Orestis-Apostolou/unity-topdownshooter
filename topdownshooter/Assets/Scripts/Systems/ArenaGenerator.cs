using UnityEngine;

public class ArenaGenerator : MonoBehaviour
{
    public float wallThickness = 1f;
    private Vector2 arenaSize = Vector2.zero;
    public GameObject wallPrefab;

    void Start()
    {
        Camera cam = Camera.main;
        float height = cam.orthographicSize * 2f;
        float width = height * (16/9f);
        arenaSize = new Vector2 (width, height);

        CreateWall("WallTop", new Vector2(0, arenaSize.y/2 + wallThickness/2), new Vector2(arenaSize.x + 2 * wallThickness, wallThickness));
        CreateWall("WallBottom", new Vector2(0, -arenaSize.y/2 - wallThickness/2), new Vector2(arenaSize.x + 2 * wallThickness, wallThickness));
        CreateWall("WallLeft", new Vector2(-arenaSize.x/2 - wallThickness/2, 0), new Vector2(wallThickness, arenaSize.y + 2 * wallThickness));
        CreateWall("WallRight", new Vector2(arenaSize.x/2 + wallThickness/2, 0), new Vector2(wallThickness, arenaSize.y + 2 * wallThickness));
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
}
