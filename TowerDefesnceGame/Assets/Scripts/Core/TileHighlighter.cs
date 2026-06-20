using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHighlighter : MonoBehaviour
{
    public static TileHighlighter Instance { get; private set; }

    public Tilemap groundTilemap;
    public Tilemap pathTilemap;
    public Color   validColor   = new Color(0f, 1f, 0f, 0.35f);
    public Color   invalidColor = new Color(1f, 0f, 0f, 0.35f);
    public Color   normalColor  = Color.white;

    private Vector3Int lastCell = new Vector3Int(999, 999, 0);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!TowerPlacer.Instance.IsPlacing)
        {
            // Clear any highlight when not placing
            if (lastCell != new Vector3Int(999, 999, 0))
            {
                groundTilemap.SetColor(lastCell, normalColor);
                lastCell = new Vector3Int(999, 999, 0);
            }
            return;
        }

        Vector3 mouse = InputHandler.Instance.MouseWorldPos;
        Vector3Int cell = groundTilemap.WorldToCell(mouse);

        if (cell == lastCell) return;

        // Reset old cell
        if (lastCell != new Vector3Int(999, 999, 0))
            groundTilemap.SetColor(lastCell, normalColor);

        // Color new cell
        bool isPath = pathTilemap.HasTile(cell);
        groundTilemap.SetColor(cell, isPath ? invalidColor : validColor);
        lastCell = cell;
    }
}