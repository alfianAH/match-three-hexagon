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
                previousSelected.Deselect(); // Deselect the previous one
                Select(); // Select the new one
            }
        }
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
