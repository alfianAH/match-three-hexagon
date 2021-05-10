using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

    public bool IsAnimating => IsSwapping;

    public bool IsSwapping { get; set; }

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

    #region Swapping
    
    /// <summary>
    /// Swap Tile A and Tile B
    /// </summary>
    /// <param name="a">Tile A</param>
    /// <param name="b">Tile B</param>
    /// <param name="onCompleted">Action on completed</param>
    /// <returns></returns>
    public IEnumerator SwapTilePosition(TileController a, TileController b, Action onCompleted)
    {
        IsSwapping = true;

        Vector2Int indexA = GetTileIndex(a);
        Vector2Int indexB = GetTileIndex(b);
        
        tiles[indexA.x, indexA.y] = b;
        tiles[indexB.x, indexB.y] = a;
        
        // Change Tile A and B's ID
        a.ChangeId(a.id, indexB.x, indexB.y);
        b.ChangeId(b.id, indexA.x, indexA.y);

        bool isRoutineACompleted = false;
        bool isRoutineBCompleted = false;
        
        // Start moving the tile with coroutine
        StartCoroutine(a.MoveTilePosition(
            GetIndexPosition(indexB), () =>
            {
                isRoutineACompleted = true;
            }));
        StartCoroutine(b.MoveTilePosition(
            GetIndexPosition(indexA), () =>
            {
                isRoutineBCompleted = true;
            }));
        
        // Wait until Tile A and B 
        yield return new WaitUntil(() => 
            isRoutineACompleted && isRoutineBCompleted);
        
        onCompleted?.Invoke();
        IsSwapping = false;
    }

    #endregion
    
    private void Start()
    {
        Vector2 tileSize = tilePrefab.GetComponent<SpriteRenderer>().size;
        CreateBoard(tileSize);
    }
    
    /// <summary>
    /// Check all tiles if there are any matching tiles
    /// </summary>
    /// <returns>
    /// All matching tiles in horizontal or vertical
    /// </returns>
    public List<TileController> GetAllMatches()
    {
        List<TileController> matchingTiles = new List<TileController>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                List<TileController> tileMatched = tiles[x, y].GetAllMatches();
                
                // Go to next tile if no match
                if(tileMatched == null || tileMatched.Count == 0) continue;

                foreach (TileController tile in tileMatched)
                {
                    // Add the one that isn't added yet
                    if (!matchingTiles.Contains(tile))
                    {
                        matchingTiles.Add(tile);
                    }
                }
            }
        }

        return matchingTiles;
    }
    
    /// <summary>
    /// Get tile's index
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>
    /// Get tile's index 
    /// Default: (-1, -1)
    /// </returns>
    public Vector2Int GetTileIndex(TileController tile)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if(tile == tiles[x, y]) return new Vector2Int(x, y);
            }
        }
        
        return new Vector2Int(-1, -1);
    }
    
    /// <summary>
    /// Convert tile's index to position
    /// </summary>
    /// <param name="index"></param>
    /// <returns>
    /// Get tile's posiion in its index
    /// </returns>
    public Vector2 GetIndexPosition(Vector2Int index)
    {
        Vector2 tileSize = tilePrefab.GetComponent<SpriteRenderer>().size;
        
        return new Vector2(startPosition.x + (tileSize.x + offsetTile.x) * index.x,
            startPosition.y + (tileSize.y + offsetTile.y) * index.y);
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
