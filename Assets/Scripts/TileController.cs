﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public int id;
    
    private BoardManager board;
    private SpriteRenderer spriteRenderer;
    
    private static readonly Color SelectedColor = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color NormalColor = Color.white;

    private static TileController previousSelected;
    
    private bool isSelected;
    private static readonly float MoveDuration = 0.5f;
    
    public bool IsDestroyed { get; private set; }
    
    private void Awake()
    {
        board = BoardManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        // Non selectible condition
        if (spriteRenderer.sprite == null) return;
        
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
    
    /// <summary>
    /// Swap tile with other tile
    /// </summary>
    /// <param name="otherTile">Other selected tile</param>
    /// <param name="onCompleted">Action on completed</param>
    public void SwapTile(TileController otherTile, Action onCompleted = null)
    {
        StartCoroutine(board.SwapTilePosition(this, otherTile, onCompleted));
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

    #region Adjacent
    
    private static readonly Vector2[] AdjacentDirection = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };
    
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
    public List<TileController> GetAllAdjacentTiles()
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
        if (matchingTiles != null && matchingTiles.Count >= 2)
        {
            matchingTiles.Add(this);
        }

        return matchingTiles;
    }

    #endregion
    
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
}