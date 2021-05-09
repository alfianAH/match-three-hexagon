using UnityEngine;

public class TileController : MonoBehaviour
{
    public int id;

    private BoardManager board;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        board = BoardManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Set tile's ID, sprite, and name
    /// </summary>
    /// <param name="id">Tile's ID</param>
    /// <param name="x">Tile's X axis</param>
    /// <param name="y">Tile's Y axis</param>
    public void ChangeId(int id, int x, int y)
    {
        spriteRenderer.sprite = board.tileTypes[id];
        this.id = id;
        name = $"TILE_{id}({x},{y})";
    }
}
