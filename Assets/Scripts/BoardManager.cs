using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
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
    
    #region Singleton

    private static BoardManager instance;

    public static BoardManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BoardManager>();

                if (instance == null)
                    Debug.LogError("BoardManager not Found");
            }

            return instance;
        }
    }

    #endregion
    
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
                // Instantiate the prefab
                GameObject newTileObject = Instantiate(tilePrefab,
                    new Vector2(startPosition.x + (tileSize.x + offsetTile.x) * x,
                        startPosition.y + (tileSize.y + offsetTile.y) * y),
                    tilePrefab.transform.rotation,
                    transform
                );
                TileController newTile = newTileObject.GetComponent<TileController>();
                
                tiles[x, y] = newTile; // Tile on x, y
                
                // Get the list of possible tile's IDs
                List<int> possibleId = GetStartingPossibleIdList(x, y);
                // Choose a random number in possible IDs
                int newId = possibleId[Random.Range(0, possibleId.Count)];

                newTile.ChangeId(newId, x, y);
            }
        }
    }
    
    /// <summary>
    /// Get starting possible tile's ID
    /// </summary>
    /// <param name="x">Axis X</param>
    /// <param name="y">Axis Y</param>
    /// <returns></returns>
    private List<int> GetStartingPossibleIdList(int x, int y)
    {
        List<int> possibleId = new List<int>();
        
        // Add all possible ID
        for (int i = 0; i < tileTypes.Count; i++)
        {
            possibleId.Add(i);
        }
        
        // If the first and second tiles on the left are the same, ...
        if (x > 1 && tiles[x-1, y].id == tiles[x-2, y].id)
        {
            // Remove the ID
            possibleId.Remove(tiles[x - 1, y].id);
        }
        
        // If the first and second tiles on top are the same, ...
        if (y > 1 && tiles[x, y - 1].id == tiles[x, y - 2].id)
        {
            // Remove the ID
            possibleId.Remove(tiles[x, y - 1].id);
        }

        return possibleId;
    }
}
