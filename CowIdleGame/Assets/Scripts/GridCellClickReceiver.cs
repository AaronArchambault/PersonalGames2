using UnityEngine;

// Attach this to your cell highlight tile prefab alongside a Collider2D.
// When the player has a cow selected and clicks an empty cell, the cow slides there.
[RequireComponent(typeof(Collider2D))]
public class GridCellClickReceiver : MonoBehaviour
{
    // Set this after instantiating the tile (GridManager does it automatically if you
    // call SetupCellReceiver after building the grid — see note below).
    public GridCell LinkedCell { get; set; }

    void OnMouseUpAsButton()
    {
        if (LinkedCell == null) return;
        DragHandler.OnCellClicked(LinkedCell);
    }
}
