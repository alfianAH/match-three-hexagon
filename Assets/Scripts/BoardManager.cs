﻿using System;
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

    public bool IsAnimating => IsProcessing || IsSwapping;

    public bool IsSwapping { get; set; }
    public bool IsProcessing { get; set; }
    
    public bool IsAllTrue(List<bool> list)
    {
        foreach (bool status in list)
        {
            if (!status) return false;
        }

        return true;
    }

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
    
    #region Match

    private void ProcessMatches()
    {
        List<TileController> matchingTiles = GetAllMatches();
        
        // Stop locking if not match found
        if (matchingTiles == null || matchingTiles.Count == 0)
        {
            IsProcessing = false;
            return;
        }

        StartCoroutine(ClearMatches(matchingTiles, ProcessDrop));
    }

    private IEnumerator ClearMatches(List<TileController> matchingTiles, Action onCompleted)
    {
        List<bool> isCompleted = new List<bool>();

        for (int i = 0; i < matchingTiles.Count; i++)
        {
            isCompleted.Add(false);
        }

        for (int i = 0; i < matchingTiles.Count; i++)
        {
            int index = i;
            StartCoroutine(matchingTiles[i].SetDestroyed(() =>
            {
                isCompleted[index] = true;
            }));
        }
        
        yield return new WaitUntil(() => IsAllTrue(isCompleted));
        
        onCompleted?.Invoke();
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

    #region Drop

    private void ProcessDrop()
    {
        Dictionary<TileController, int> droppingTiles = GetAllDrop();

        StartCoroutine(DropTiles(droppingTiles, ProcessDestroyAndFill));
    }

    private Dictionary<TileController, int> GetAllDrop()
    {
        Dictionary<TileController, int> droppingTiles = new Dictionary<TileController, int>();
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (tiles[x, y].IsDestroyed)
                {
                    // Process for all tile on top of destroyed tile
                    for (int i = y+1; i < size.y; i++)
                    {
                        if(tiles[x, i].IsDestroyed) continue;
                        
                        // If this tile already on drop list, increase its drop range
                        if (droppingTiles.ContainsKey(tiles[x, i]))
                        {
                            droppingTiles[tiles[x, i]]++;
                        }
                        // If not on drop list, add it with drop range one
                        else
                        {
                            droppingTiles.Add(tiles[x, i], 1);
                        }
                    }
                }
            }
        }

        return droppingTiles;
    }

    private IEnumerator DropTiles(Dictionary<TileController, int> droppingTiles, Action onCompleted)
    {
        foreach (KeyValuePair<TileController,int> pair in droppingTiles)
        {
            Vector2Int tileIndex = GetTileIndex(pair.Key);

            TileController temp = pair.Key;
            tiles[tileIndex.x, tileIndex.y] = tiles[tileIndex.x, tileIndex.y - pair.Value];
            tiles[tileIndex.x, tileIndex.y - pair.Value] = temp;
            
            temp.ChangeId(temp.id, tileIndex.x, tileIndex.y - pair.Value);
        }

        yield return null;
        
        onCompleted?.Invoke();
    }

    #endregion
    
    #region Destroy and Fill

    private void ProcessDestroyAndFill()
    {
        List<TileController> destroyedTiles = GetAllDestroyed();

        StartCoroutine(DestroyAndFillTiles(destroyedTiles, ProcessReposition));
    }
    
    /// <summary>
    /// Get all destroyed tiles
    /// </summary>
    /// <returns>All destroyed tiles</returns>
    private List<TileController> GetAllDestroyed()
    {
        List<TileController> destroyedTiles = new List<TileController>();
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (tiles[x, y].IsDestroyed)
                {
                    destroyedTiles.Add(tiles[x, y]);
                }
            }
        }

        return destroyedTiles;
    }

    private IEnumerator DestroyAndFillTiles(List<TileController> destroyedTiles, Action onCompleted)
    {
        List<int> highestIndex = new List<int>();

        for (int i = 0; i < size.x; i++)
        {
            highestIndex.Add(size.y - 1);
        }

        float spawnHeight = endPosition.y + tilePrefab.GetComponent<SpriteRenderer>().size.y + offsetTile.y;

        foreach (TileController tile in destroyedTiles)
        {
            Vector2Int tileIndex = GetTileIndex(tile);
            Vector2Int targetIndex = new Vector2Int(tileIndex.x, highestIndex[tileIndex.x]);
            highestIndex[tileIndex.x]--;

            tile.transform.position = new Vector2(tile.transform.position.x, spawnHeight);
            tile.GenerateRandomTile(targetIndex.x, targetIndex.y);
        }

        yield return null;
        
        onCompleted?.Invoke();
    }

    #endregion

    #region Reposition

    private void ProcessReposition()
    {
        StartCoroutine(RepositionTiles(ProcessMatches));
    }

    private IEnumerator RepositionTiles(Action onComplete)
    {
        List<bool> isCompleted = new List<bool>();

        int i = 0;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2 targetPosition = GetIndexPosition(new Vector2Int(x, y));
                
                // Skip if already on position
                if ((Vector2) tiles[x, y].transform.position == targetPosition) 
                    continue;
                
                isCompleted.Add(false);

                int index = i;
                StartCoroutine(tiles[x, y].MoveTilePosition(targetPosition, () =>
                {
                    isCompleted[index] = true;
                }));

                i++;
            }
        }

        yield return new WaitUntil(() => IsAllTrue(isCompleted));
        
        onComplete?.Invoke();
    }

    #endregion

    private void Start()
    {
        Vector2 tileSize = tilePrefab.GetComponent<SpriteRenderer>().size;
        CreateBoard(tileSize);
        
        IsSwapping = false;
        IsProcessing = false;
    }
    
    public void Process()
    {
        IsProcessing = true;
        ProcessMatches();
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
