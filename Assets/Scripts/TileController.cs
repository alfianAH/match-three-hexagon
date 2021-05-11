using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileController : MonoBehaviour
{
    private static readonly Color SelectedColor = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color NormalColor = Color.white;
    
    private static readonly float MoveDuration = 0.5f;
    private static readonly float DestroyBigDuration = 0.1f;
    private static readonly float DestroySmallDuration = 0.4f;

    private static readonly Vector2 SizeBig = Vector2.one * 1.2f;
    private static readonly Vector2 SizeSmall = Vector2.zero;
    private static readonly Vector2 SizeNormal = Vector2.one;
    
    private static readonly Vector2[] AdjacentDirection = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };
    
    private static TileController previousSelected;
    
    public int id;
    
    private BoardManager board;
    private GameFlowManager gameFlowManager;
    private SpriteRenderer spriteRenderer;
    
    private bool isSelected;

    #region Setter and Getter

    public bool IsDestroyed { get; private set; }

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        board = BoardManager.Instance;
        gameFlowManager = GameFlowManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        IsDestroyed = false;
    }

    private void OnMouseDown()
    {
        // Non selectible condition
        if (spriteRenderer.sprite == null || board.IsAnimating || gameFlowManager.IsGameOver) 
            return;
        
        // When mouse is clicked on the tile
        // If tile is selected, ...
        if (isSelected)
        {
            Deselect(); // Deselect the tile
        }
        else 
        {
            // If nothing is selected yet, ...
            if (previousSelected == null)
            {
                Select(); // Select the tile
            }
            else  // If there is one, ...
            {
                // If previous selected is in adjacent tiles, ...
                if(GetAllAdjacentTiles().Contains(previousSelected))
                {
                    TileController otherTile = previousSelected;
                    // Swap tile
                    SwapTile(otherTile, () =>
                    {
                        if (board.GetAllMatches().Count > 0)
                        {
                            Debug.Log("MATCH FOUND");
                            board.Process();
                        }
                        else
                        {
                            SwapTile(otherTile);
                        }
                    });
                } 
                else // If not on adjacent tiles, ...
                {
                    previousSelected.Deselect(); // Deselect the previous one
                    Select(); // Select the new one
                }
            }
        }
    }

    #endregion

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

    #region Select & Deselect

    /// <summary>
    /// Tile's condition when it's selected
    /// </summary>
    private void Select()
    {
        isSelected = true;
        spriteRenderer.color = SelectedColor;
        previousSelected = this;
    }

    /// <summary>
    /// Tile's condition when it's deselected
    /// </summary>
    private void Deselect()
    {
        isSelected = false;
        spriteRenderer.color = NormalColor;
        previousSelected = null;
    }

    #endregion
    
    #region Swapping and Moving
    
    /// <summary>
    /// Swap tile with other tile
    /// </summary>
    /// <param name="otherTile">Other selected tile</param>
    /// <param name="onCompleted">Action on completed</param>
    private void SwapTile(TileController otherTile, Action onCompleted = null)
    {
        StartCoroutine(board.SwapTilePosition(this, otherTile, onCompleted));
    }

    /// <summary>
    /// Move tile's position to target position
    /// </summary>
    /// <param name="targetPosition">Target position</param>
    /// <param name="onCompleted">Action on completed</param>
    /// <returns></returns>
    public IEnumerator MoveTilePosition(Vector2 targetPosition, Action onCompleted)
    {
        Vector2 startPosition = transform.position;
        float time = 0f;
        
        // Run animation on next frame for safety reason
        yield return new WaitForEndOfFrame();

        while (time < MoveDuration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, time / MoveDuration);

            time += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }

        transform.position = targetPosition;
        
        onCompleted.Invoke();
    }
    
    #endregion

    #region Adjacent
    
    /// <summary>
    /// Get adjacent tile
    /// </summary>
    /// <param name="raycastDir">Raycast direction</param>
    /// <returns>
    /// Tile that hit by raycast 
    /// </returns>
    private TileController GetAdjacent(Vector2 raycastDir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDir, spriteRenderer.size.x);
        
        // If hit, return tile
        if (hit)
        {
            return hit.collider.GetComponent<TileController>();
        }

        return null;
    }
    
    /// <summary>
    /// Get all adjacent tiles that are on top, bottom, left, right
    /// </summary>
    /// <returns>
    /// Tiles that are on top, bottom, left, right
    /// </returns>
    private List<TileController> GetAllAdjacentTiles()
    {
        List<TileController> adjacentTiles = new List<TileController>();

        foreach (Vector2 adjacentDirection in AdjacentDirection)
        {
            adjacentTiles.Add(GetAdjacent(adjacentDirection));
        }

        return adjacentTiles;
    }

    #endregion

    #region Check Match
    
    /// <summary>
    /// Get all tiles that match
    /// </summary>
    /// <param name="raycastDir">Raycast direction</param>
    /// <returns></returns>
    private List<TileController> GetMatch(Vector2 raycastDir)
    {
        List<TileController> matchingTiles = new List<TileController>();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDir, spriteRenderer.size.x);

        while (hit)
        {
            // Get other tile
            TileController otherTile = hit.collider.GetComponent<TileController>();
            
            if (otherTile.id != id || otherTile.IsDestroyed) break;
            
            // Add other tile to list
            matchingTiles.Add(otherTile);
            // Raycast on other tile
            hit = Physics2D.Raycast(otherTile.transform.position, raycastDir, spriteRenderer.size.x);
        }

        return matchingTiles;
    }
    
    /// <summary>
    /// Get one line match (Vertical or Horizontal)
    /// </summary>
    /// <param name="paths">Vector for Vertical or Horizontal</param>
    /// <returns>
    /// Match tiles in 1 line
    /// </returns>
    private List<TileController> GetOneLineMatch(Vector2[] paths)
    {
        List<TileController> matchingTiles = new List<TileController>();
        
        foreach (Vector2 path in paths)
        {
            // Add match tile to the list
            matchingTiles.AddRange(GetMatch(path));
        }
        
        // Match if matching tiles are more or equal to 2 (include itself) 
        if (matchingTiles.Count >= 2)
        {
            return matchingTiles;
        }

        return null;
    }
    
    /// <summary>
    /// Get all matches in horizontal or vertical
    /// </summary>
    /// <returns>
    /// All match tiles in horizontal or vertical
    /// </returns>
    public List<TileController> GetAllMatches()
    {
        if (IsDestroyed) return null;
        
        List<TileController> matchingTiles = new List<TileController>();
        
        // Get matches for horizontal and vertical
        List<TileController> horizontalMatchingTiles = GetOneLineMatch(new[]
        {
            Vector2.up, Vector2.down
        });
        
        List<TileController> verticalMatchingTiles = GetOneLineMatch(new[]
        {
            Vector2.left, Vector2.right
        });
        
        // Add matching tiles to the list
        if (horizontalMatchingTiles != null)
        {
            matchingTiles.AddRange(horizontalMatchingTiles);
        }

        if (verticalMatchingTiles != null)
        {
            matchingTiles.AddRange(verticalMatchingTiles);
        }
        
        // Add itself to match tiles if match found
        if (matchingTiles.Count >= 2)
        {
            matchingTiles.Add(this);
        }

        return matchingTiles;
    }

    #endregion

    #region Destroy and Generate

    /// <summary>
    /// Play destroyed tile animation
    /// </summary>
    /// <param name="onCompleted">Action on completed</param>
    /// <returns></returns>
    public IEnumerator SetDestroyed(Action onCompleted)
    {
        IsDestroyed = true;
        id = -1;
        name = "TILE_NULL";

        Vector2 startSize = transform.localScale;
        float time = 0.0f;
        
        // Make it bigger
        while (time < DestroyBigDuration)
        {
            transform.localScale = Vector2.Lerp(startSize, SizeBig, time / DestroyBigDuration);

            time += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }

        Transform tileTransform = transform;
        
        tileTransform.localScale = SizeBig;
        startSize = tileTransform.localScale;
        time = 0.0f;
        
        // Make it smaller
        while (time < DestroyBigDuration)
        {
            transform.localScale = Vector2.Lerp(startSize, SizeSmall, time / DestroySmallDuration);

            time += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }

        transform.localScale = SizeSmall;
        
        // Destroy the tile
        spriteRenderer.sprite = null;
        onCompleted?.Invoke();
    }

    /// <summary>
    /// Generate random tile
    /// </summary>
    /// <param name="x">Axis X</param>
    /// <param name="y">Axis Y</param>
    public void GenerateRandomTile(int x, int y)
    {
        transform.localScale = SizeNormal;
        IsDestroyed = false;
        ChangeId(Random.Range(0, board.tileTypes.Count), x, y);
    }
    
    #endregion
}
