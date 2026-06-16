using UnityEngine;

// Attach this to each cow prefab alongside CowEntity.
// Requires a Collider2D (e.g. CircleCollider2D) on the same GameObject.
[RequireComponent(typeof(CowEntity))]
public class DragHandler : MonoBehaviour
{
    CowEntity _entity;
    Camera    _cam;
    bool      _dragging;
    GridCell  _originCell;
    Vector3   _offset;

    void Awake()
    {
        _entity = GetComponent<CowEntity>();
        _cam    = Camera.main;
    }

    // ── Unity mouse events ────────────────────────────────────────────────────
    void OnMouseDown()
    {
        _dragging   = true;
        _originCell = _entity.CurrentCell;
        _originCell.Occupant = null;          // free the cell while dragging
        _entity.OnPickUp();

        // Offset so the sprite doesn't jump to cursor center
        _offset = transform.position - GetMouseWorld();
    }

    void OnMouseDrag()
    {
        if (!_dragging) return;
        Vector3 target = GetMouseWorld() + _offset;
        target.z       = -1f;               // in front of grid
        transform.position = target;

        HighlightCellUnderCursor();
    }

    void OnMouseUp()
    {
        if (!_dragging) return;
        _dragging = false;
        ClearHighlights();
        HandleDrop();
    }

    // ── Drop logic ────────────────────────────────────────────────────────────
    void HandleDrop()
    {
        GridCell dropCell = GridManager.Instance.GetCellAtWorld(GetMouseWorld());

        // Dropped outside grid — snap back
        if (dropCell == null)
        {
            SnapBack();
            return;
        }

        // Dropped on occupied cell — attempt merge
        if (!dropCell.IsEmpty)
        {
            bool merged = MergeManager.Instance.TryMerge(_entity, dropCell.Occupant);
            if (!merged) SnapBack();  // merge failed, return home
            return;
        }

        // Dropped on empty cell — move there
        dropCell.Occupant = _entity;
        _entity.OnPutDown(dropCell);
    }

    void SnapBack()
    {
        _originCell.Occupant = _entity;
        _entity.OnPutDown(_originCell);
    }

    // ── Visual feedback ───────────────────────────────────────────────────────
    GridCell _lastHighlighted;

    void HighlightCellUnderCursor()
    {
        GridCell cell = GridManager.Instance.GetCellAtWorld(GetMouseWorld());
        if (cell == _lastHighlighted) return;

        // Clear old highlight
        if (_lastHighlighted != null)
            SetCellHighlight(_lastHighlighted, false);

        _lastHighlighted = cell;
        if (cell != null)
            SetCellHighlight(cell, true);
    }

    void ClearHighlights()
    {
        if (_lastHighlighted != null)
            SetCellHighlight(_lastHighlighted, false);
        _lastHighlighted = null;
    }

    // Override this if you have a separate highlight child object on each cell tile
    void SetCellHighlight(GridCell cell, bool on)
    {
        // Simple color pulse on nearby cows that can merge
        if (!cell.IsEmpty && cell.Occupant.Data.tier == _entity.Data.tier)
        {
            cell.Occupant.GetComponent<SpriteRenderer>().color =
                on ? Color.yellow : cell.Occupant.Data.tintColor;
        }
    }

    // ── Utility ───────────────────────────────────────────────────────────────
    Vector3 GetMouseWorld()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = Mathf.Abs(_cam.transform.position.z);
        return _cam.ScreenToWorldPoint(mp);
    }
}
