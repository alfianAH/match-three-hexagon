using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    [Header("Board")] 
    public Vector2Int size;
    public Vector2 offsetTile;
    public Vector2 offsetBoard;

    [Header("Tile")] 
    public List<Sprite> tileTypes = new List<Sprite>();
    public GameObject tilePrefab;

    private Vector2 startPosition,
        endPosition;
    private TileController[,] tiles;

    private void Start()
    {
        Vector2 tileSize = tilePrefab.GetComponent<SpriteRenderer>().size;
        CreateBoard(tileSize);
    }
    
    /// <summary>
    /// Create board for the game
    /// </summary>
    /// <param name="tileSize">Tile prefab's size</param>
    private void CreateBoard(Vector2 tileSize)
    {
        tiles = new TileController[size.x, size.y];

        Vector2 totalSize = (tileSize + offsetTile) * (size - Vector2Int.one);

        startPosition = (Vector2) transform.position - totalSize / 2 + offsetBoard;
        endPosition = startPosition + totalSize;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                TileController newTile = Instantiate(tilePrefab,
                    new Vector2(startPosition.x + ((tileSize.x + offsetTile.x) * x),
                        startPosition.y + ((tileSize.y + offsetTile.y) * y)),
                    tilePrefab.transform.rotation,
                    transform
                    ).GetComponent<TileController>();

                tiles[x, y] = newTile;
            }
        }
    }
}
