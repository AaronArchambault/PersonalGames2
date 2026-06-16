using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int columns = 5;
    public int rows    = 4;
    public float cellSize = 1.8f;
    public Vector2 originOffset = Vector2.zero;

    [Header("Prefabs")]
    public GameObject cellHighlightPrefab;  // visual tile under cows
    public GameObject cowPrefab;            // base prefab with CowEntity

    [Header("Auto-Spawn")]
    public float spawnIntervalSeconds = 8f;
    public int   maxAutoSpawnTier     = 0;  // only spawn basic cows automatically

    // ── Internal state ────────────────────────────────────────────────────────
    GridCell[,] _grid;
    float       _spawnTimer;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        BuildGrid();
        LoadGrid();
        StartCoroutine(AutoSpawnRoutine());
    }

    // ── Grid construction ─────────────────────────────────────────────────────
    void BuildGrid()
    {
        _grid = new GridCell[columns, rows];
        for (int x = 0; x < columns; x++)
        for (int y = 0; y < rows;    y++)
        {
            Vector3 pos = CellToWorld(x, y);
            _grid[x, y] = new GridCell(x, y, pos);

            if (cellHighlightPrefab)
            {
                var tile = Instantiate(cellHighlightPrefab, pos, Quaternion.identity, transform);
                var receiver = tile.GetComponent<GridCellClickReceiver>();
                if (receiver) receiver.LinkedCell = _grid[x, y];
            }
        }
    }

    public Vector3 CellToWorld(int x, int y)
    {
        float startX = -(columns - 1) * cellSize * 0.5f + originOffset.x;
        float startY = -(rows    - 1) * cellSize * 0.5f + originOffset.y;
        return new Vector3(startX + x * cellSize, startY + y * cellSize, 0f);
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────
    public CowEntity SpawnCow(int tier, int x = -1, int y = -1)
    {
        CowData data = GameManager.Instance.database.GetTier(tier);
        if (data == null) return null;

        GridCell cell = (x >= 0 && y >= 0) ? _grid[x, y] : GetRandomEmptyCell();
        if (cell == null) return null;

        GameObject go = Instantiate(cowPrefab, cell.worldPosition, Quaternion.identity, transform);
        CowEntity entity = go.GetComponent<CowEntity>();
        entity.Initialize(data, cell);
        cell.Occupant = entity;

        GameManager.Instance.RecalculateCPS();
        return entity;
    }

    IEnumerator AutoSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnIntervalSeconds);
            if (HasEmptyCell())
                SpawnCow(Random.Range(0, maxAutoSpawnTier + 1));
        }
    }

    // ── Cell helpers ──────────────────────────────────────────────────────────
    public GridCell GetCell(int x, int y)
    {
        if (x < 0 || x >= columns || y < 0 || y >= rows) return null;
        return _grid[x, y];
    }

    public GridCell GetCellAtWorld(Vector3 worldPos)
    {
        GridCell closest = null;
        float bestDist = float.MaxValue;
        foreach (var cell in _grid)
        {
            float d = Vector3.Distance(worldPos, cell.worldPosition);
            if (d < bestDist && d < cellSize * 0.6f)
            { bestDist = d; closest = cell; }
        }
        return closest;
    }

    public GridCell GetRandomEmptyCell()
    {
        var empty = new List<GridCell>();
        foreach (var cell in _grid)
            if (cell.IsEmpty) empty.Add(cell);
        if (empty.Count == 0) return null;
        return empty[Random.Range(0, empty.Count)];
    }

    public bool HasEmptyCell()
    {
        foreach (var cell in _grid)
            if (cell.IsEmpty) return true;
        return false;
    }

    // Called by PrestigeManager to wipe the board
    public void ClearAllCells()
    {
        foreach (var cell in _grid)
            cell.Occupant = null;
        PlayerPrefs.DeleteKey("gridState");
    }

    // ── Save / Load ───────────────────────────────────────────────────────────
    const string KEY_GRID = "gridState";

    public void SaveGrid()
    {
        var state = new List<string>();
        foreach (var cell in _grid)
        {
            if (!cell.IsEmpty)
                state.Add($"{cell.x},{cell.y},{cell.Occupant.Data.tier}");
        }
        PlayerPrefs.SetString(KEY_GRID, string.Join("|", state));
    }

    void LoadGrid()
    {
        if (!PlayerPrefs.HasKey(KEY_GRID)) return;
        string raw = PlayerPrefs.GetString(KEY_GRID);
        if (string.IsNullOrEmpty(raw)) return;
        foreach (string entry in raw.Split('|'))
        {
            string[] parts = entry.Split(',');
            if (parts.Length == 3)
                SpawnCow(int.Parse(parts[2]), int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}

// ── GridCell (plain class, not MonoBehaviour) ─────────────────────────────────
[System.Serializable]
public class GridCell
{
    public int         x, y;
    public Vector3     worldPosition;
    public CowEntity   Occupant;
    public bool        IsEmpty => Occupant == null;

    public GridCell(int x, int y, Vector3 pos)
    { this.x = x; this.y = y; worldPosition = pos; }
}




/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int columns = 5;
    public int rows    = 4;
    public float cellSize = 1.8f;
    public Vector2 originOffset = Vector2.zero;

    [Header("Prefabs")]
    public GameObject cellHighlightPrefab;  // visual tile under cows
    public GameObject cowPrefab;            // base prefab with CowEntity

    [Header("Auto-Spawn")]
    public float spawnIntervalSeconds = 8f;
    public int   maxAutoSpawnTier     = 0;  // only spawn basic cows automatically

    // ── Internal state ────────────────────────────────────────────────────────
    GridCell[,] _grid;
    float       _spawnTimer;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        BuildGrid();
        LoadGrid();
        StartCoroutine(AutoSpawnRoutine());
    }

    // ── Grid construction ─────────────────────────────────────────────────────
    void BuildGrid()
    {
        _grid = new GridCell[columns, rows];
        for (int x = 0; x < columns; x++)
        for (int y = 0; y < rows;    y++)
        {
            Vector3 pos = CellToWorld(x, y);
            _grid[x, y] = new GridCell(x, y, pos);

            if (cellHighlightPrefab)
                Instantiate(cellHighlightPrefab, pos, Quaternion.identity, transform);
        }
    }

    public Vector3 CellToWorld(int x, int y)
    {
        float startX = -(columns - 1) * cellSize * 0.5f + originOffset.x;
        float startY = -(rows    - 1) * cellSize * 0.5f + originOffset.y;
        return new Vector3(startX + x * cellSize, startY + y * cellSize, 0f);
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────
    public CowEntity SpawnCow(int tier, int x = -1, int y = -1)
    {
        CowData data = GameManager.Instance.database.GetTier(tier);
        if (data == null) return null;

        GridCell cell = (x >= 0 && y >= 0) ? _grid[x, y] : GetRandomEmptyCell();
        if (cell == null) return null;

        GameObject go = Instantiate(cowPrefab, cell.worldPosition, Quaternion.identity, transform);
        CowEntity entity = go.GetComponent<CowEntity>();
        entity.Initialize(data, cell);
        cell.Occupant = entity;

        GameManager.Instance.RecalculateCPS();
        return entity;
    }

    IEnumerator AutoSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnIntervalSeconds);
            if (HasEmptyCell())
                SpawnCow(Random.Range(0, maxAutoSpawnTier + 1));
        }
    }

    // ── Cell helpers ──────────────────────────────────────────────────────────
    public GridCell GetCell(int x, int y)
    {
        if (x < 0 || x >= columns || y < 0 || y >= rows) return null;
        return _grid[x, y];
    }

    public GridCell GetCellAtWorld(Vector3 worldPos)
    {
        GridCell closest = null;
        float bestDist = float.MaxValue;
        foreach (var cell in _grid)
        {
            float d = Vector3.Distance(worldPos, cell.worldPosition);
            if (d < bestDist && d < cellSize * 0.6f)
            { bestDist = d; closest = cell; }
        }
        return closest;
    }

    public GridCell GetRandomEmptyCell()
    {
        var empty = new List<GridCell>();
        foreach (var cell in _grid)
            if (cell.IsEmpty) empty.Add(cell);
        if (empty.Count == 0) return null;
        return empty[Random.Range(0, empty.Count)];
    }

    public bool HasEmptyCell()
    {
        foreach (var cell in _grid)
            if (cell.IsEmpty) return true;
        return false;
    }

    // Called by PrestigeManager to wipe the board
    public void ClearAllCells()
    {
        foreach (var cell in _grid)
            cell.Occupant = null;
        PlayerPrefs.DeleteKey("gridState");
    }

    // ── Save / Load ───────────────────────────────────────────────────────────
    const string KEY_GRID = "gridState";

    public void SaveGrid()
    {
        var state = new List<string>();
        foreach (var cell in _grid)
        {
            if (!cell.IsEmpty)
                state.Add($"{cell.x},{cell.y},{cell.Occupant.Data.tier}");
        }
        PlayerPrefs.SetString(KEY_GRID, string.Join("|", state));
    }

    void LoadGrid()
    {
        if (!PlayerPrefs.HasKey(KEY_GRID)) return;
        string raw = PlayerPrefs.GetString(KEY_GRID);
        if (string.IsNullOrEmpty(raw)) return;
        foreach (string entry in raw.Split('|'))
        {
            string[] parts = entry.Split(',');
            if (parts.Length == 3)
                SpawnCow(int.Parse(parts[2]), int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}

// ── GridCell (plain class, not MonoBehaviour) ─────────────────────────────────
[System.Serializable]
public class GridCell
{
    public int         x, y;
    public Vector3     worldPosition;
    public CowEntity   Occupant;
    public bool        IsEmpty => Occupant == null;

    public GridCell(int x, int y, Vector3 pos)
    { this.x = x; this.y = y; worldPosition = pos; }
}*/




