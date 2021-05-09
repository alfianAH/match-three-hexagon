using System;
using System.Collections;
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
                TileController otherTile = previousSelected;
                // Swap tile
                SwapTile(otherTile, () =>
                {
                    SwapTile(otherTile);
                });
                
                // previousSelected.Deselect(); // Deselect the previous one
                // Select(); // Select the new one
            }
        }
    }
    
    /// <summary>
    /// Swap tile with other tile
    /// </summary>
    /// <param name="otherTile"></param>
    /// <param name="onCompleted"></param>
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
