using UnityEngine;
using UnityEngine.InputSystem;

// Attach to each cow prefab alongside CowEntity.
// Uses the NEW Unity Input System (not the legacy Input class).
[RequireComponent(typeof(CowEntity))]
public class DragHandler : MonoBehaviour
{
    [Header("Drag Settings")]
    [Tooltip("Pixels mouse must move before it counts as a drag vs a click")]
    public float dragThreshold = 8f;
    [Tooltip("How smoothly the cow follows the cursor while dragging")]
    public float dragFollowSpeed = 20f;

    CowEntity _entity;
    Camera    _cam;

    bool     _mouseDown;
    bool     _dragging;
    Vector3  _mouseDownWorld;
    Vector3  _dragOffset;
    GridCell _originCell;

    // Click-to-move — only one cow selected at a time
    static DragHandler _selected;
    bool _isSelected;

    void Awake()
    {
        _entity = GetComponent<CowEntity>();
        _cam    = Camera.main;
    }

    // ── Input System mouse events (OnMouseDown/Drag/Up still work with new Input System
    //    when "Both" is selected, but we read position via Mouse.current) ──────

    void OnMouseDown()
    {
        _mouseDown      = true;
        _mouseDownWorld = GetMouseWorld();
        _dragOffset     = transform.position - _mouseDownWorld;
        _originCell     = _entity.CurrentCell;
    }

    void OnMouseDrag()
    {
        if (!_mouseDown) return;

        if (!_dragging)
        {
            // Measure screen-space movement to decide if this is a drag
            Vector2 screenNow  = Mouse.current.position.ReadValue();
            Vector2 screenDown = _cam.WorldToScreenPoint(_mouseDownWorld);
            if (Vector2.Distance(screenNow, screenDown) > dragThreshold)
            {
                _dragging = true;
                Deselect();
                _originCell.Occupant = null;
                _entity.OnPickUp();
            }
        }

        if (_dragging)
        {
            Vector3 target = GetMouseWorld() + _dragOffset;
            target.z = -1f;
            transform.position = Vector3.Lerp(
                transform.position, target, Time.deltaTime * dragFollowSpeed);
            HighlightCellUnderCursor();
        }
    }

    void OnMouseUp()
    {
        if (!_mouseDown) return;
        _mouseDown = false;

        if (_dragging)
        {
            _dragging = false;
            ClearHighlights();
            HandleDrop();
        }
        else
        {
            HandleClick();
        }
    }

    // ── Click-to-move ─────────────────────────────────────────────────────────
    void HandleClick()
    {
        if (_isSelected) { Deselect(); return; }
        if (_selected != null) _selected.Deselect();
        Select();
    }

    void Select()
    {
        _isSelected = true;
        _selected   = this;
        _entity.spriteRenderer.color        = Color.Lerp(_entity.Data.tintColor, Color.white, 0.4f);
        _entity.spriteRenderer.sortingOrder = 5;
    }

    void Deselect()
    {
        _isSelected = false;
        if (_selected == this) _selected = null;
        _entity.spriteRenderer.color        = _entity.Data.tintColor;
        _entity.spriteRenderer.sortingOrder = 1;
    }

    public static void OnCellClicked(GridCell cell)
    {
        if (_selected == null || !cell.IsEmpty) return;
        if (_selected._entity.IsMoving || _selected._entity.IsDragging) return;

        _selected._entity.CurrentCell.Occupant = null;
        cell.Occupant = _selected._entity;
        _selected._entity.SlideToCell(cell);
        _selected.Deselect();
    }

    // ── Drag drop ─────────────────────────────────────────────────────────────
    void HandleDrop()
    {
        GridCell dropCell = GridManager.Instance.GetCellAtWorld(GetMouseWorld());

        if (dropCell == null) { SnapBack(); return; }

        if (!dropCell.IsEmpty)
        {
            bool merged = MergeManager.Instance.TryMerge(_entity, dropCell.Occupant);
            if (!merged) SnapBack();
            return;
        }

        dropCell.Occupant = _entity;
        _entity.OnPutDown(dropCell);
    }

    void SnapBack()
    {
        _originCell.Occupant = _entity;
        _entity.OnPutDown(_originCell);
    }

    // ── Highlights ────────────────────────────────────────────────────────────
    GridCell _lastHighlighted;

    void HighlightCellUnderCursor()
    {
        GridCell cell = GridManager.Instance.GetCellAtWorld(GetMouseWorld());
        if (cell == _lastHighlighted) return;
        if (_lastHighlighted != null) SetHighlight(_lastHighlighted, false);
        _lastHighlighted = cell;
        if (cell != null) SetHighlight(cell, true);
    }

    void ClearHighlights()
    {
        if (_lastHighlighted != null) SetHighlight(_lastHighlighted, false);
        _lastHighlighted = null;
    }

    void SetHighlight(GridCell cell, bool on)
    {
        if (!cell.IsEmpty && cell.Occupant.Data.tier == _entity.Data.tier)
            cell.Occupant.spriteRenderer.color =
                on ? Color.yellow : cell.Occupant.Data.tintColor;
    }

    // ── World position from mouse ─────────────────────────────────────────────
    Vector3 GetMouseWorld()
    {
        // New Input System: read from Mouse.current
        Vector2 screenPos = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : Vector2.zero;

        Vector3 mp = new Vector3(screenPos.x, screenPos.y,
            Mathf.Abs(_cam.transform.position.z));
        return _cam.ScreenToWorldPoint(mp);
    }
}





/*using UnityEngine;

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
*/